<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <!-- Update version -->
  <xsl:template match="profile">
    <profile version="2.0">
      <xsl:apply-templates select="node()"/>
    </profile>
  </xsl:template>

  <!-- Rename actions -->
  <xsl:template match="ToggleMouseButtonAction">
    <MouseButtonAction actiontype="ToggleMouseButton">
      <xsl:apply-templates select="@*|node()"/>
    </MouseButtonAction>
  </xsl:template>

  <!-- Cascade processing -->
  <xsl:template match="@*|*">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>