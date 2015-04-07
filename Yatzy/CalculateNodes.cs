using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace Yatzy
{
    public partial class CalculateNodes : Form
    {
        private readonly FiveDice _game;
        private readonly CalcNames _argument;

        public CalculateNodes(FiveDice game, int Nodes, string ExpectFilename, string VarianceFilename)
        {
            InitializeComponent();
            _game = game;
            _argument = new CalcNames {Nodes = Nodes, eFilename = ExpectFilename, vFilename = VarianceFilename};
            backgroundWorker1.DoWork += BackgroundWorker1OnDoWork;
            backgroundWorker1.ProgressChanged += BackgroundWorker1OnProgressChanged;
            backgroundWorker1.RunWorkerCompleted += BackgroundWorker1OnRunWorkerCompleted;
        }

        public bool Start()
        {
            backgroundWorker1.RunWorkerAsync(_argument);
            return true;
        }

        private void BackgroundWorker1OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            Close();
        }

        private void BackgroundWorker1OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void BackgroundWorker1OnDoWork(object sender, DoWorkEventArgs ea)
        {
            BackgroundWorker bw = (BackgroundWorker)sender;

            var names = (CalcNames) ea.Argument;
            int Nodes = names.Nodes;
            string ExpectFilename = names.eFilename;
            string VarianceFilename = names.vFilename;


            bw.ReportProgress(0);
            using (var eos = new FileStream(ExpectFilename, FileMode.Create))
            using (var vos = new FileStream(VarianceFilename, FileMode.Create))
            using (var esw = new BinaryWriter(eos))
            using (var vsw = new BinaryWriter(vos))
            {
                int NodeNo;
                int[] n = new int[6];

                int[,] UnusedI = new int[_game.UsableItems, 2];

                for (NodeNo = 1; NodeNo <= Nodes; NodeNo++)
                {
                    var ActiveI = 0;
                    for (int i = 0; i < _game.UsableItems; i++)
                    {
                        int d = _game.ActiveItem(NodeNo, i);
                        if (d > 0)
                        {
                            UnusedI[ActiveI, 0] = i + 1;
                            UnusedI[ActiveI, 1] = d;
                            ActiveI++;
                        }
                    }

                    if (_game.Expect[NodeNo] == 0)
                    {
                        _game.GamePlan(6, n, UnusedI, NodeNo, ActiveI, 0);
                        float e = (float)_game.Expect[NodeNo];
                        try
                        {
                            esw.Write(e);
                        }
                        catch (IOException)
                        {
                            throw new ApplicationException("Trouble with node " + NodeNo);
                        }
                        if (FiveDice.varians)
                        {
                            float v = (float)_game.Vari[NodeNo];
                            //						pout.print(" " + ((System.Double)v).ToString("F4"));
                            try
                            {
                                vsw.Write(v);
                                //							if (NodeNo % 16 == 0) 
                                //								vsw.Flush();
                            }
                            catch (IOException)
                            {
                                throw new ApplicationException("Double Trouble with node " + NodeNo);
                            }
                        }
                    }
                    if (bw.CancellationPending)
                    {
                        bw.CancelAsync();
                    }
                    bw.ReportProgress(NodeNo * 100 / Nodes);

                }

                esw.Flush();
                vsw.Flush();
            }
        }

        private void CalculateNodes_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (backgroundWorker1.IsBusy)
            {
                backgroundWorker1.CancelAsync();
            }
        }

        
    }

    public class CalcNames
    {
        public int Nodes;
        public string eFilename;
        public string vFilename;
    }
}
