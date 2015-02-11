<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:template match="/">
	<html>
		<body>
			<hl>PhD Game Scores</hl>
			<hr />
			<table width="100%" border="1">
				<tr bgcolor="gainsboro">
					<td><b>Game</b></td>
					<td><b>Player</b></td>
					<td><b>Commenced</b></td>
					<td><b>Ended</b></td>
					<td><b>Point</b></td>
					<td><b>Bonus</b></td>
				</tr>
				<xsl:for-each select="Scores/Score">
				<xsl:sort select="Game" order="ascending"/>
				<xsl:sort select="Bonus" data-type="number" order="descending"/>
				<xsl:sort select="Point" data-type="number" order="descending"/>
					<tr>
						<td><xsl:value-of select="Game" /></td>
						<td><xsl:value-of select="Player" /></td>
						<td><xsl:value-of select="Commenced" /></td>
						<td><xsl:value-of select="Ended" /></td>
						<td><xsl:value-of select="Point" /></td>
						<td><xsl:value-of select="Bonus" /></td>
					</tr>
				</xsl:for-each>
			</table>
		</body>
	</html>
</xsl:template>
</xsl:stylesheet>

  