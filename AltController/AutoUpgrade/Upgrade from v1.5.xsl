<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <!-- Update version -->
  <xsl:template match="profile">
    <profile version="1.6">
      <xsl:apply-templates select="node()"/>
    </profile>
  </xsl:template>

  <!-- Delete controller sources -->
  <xsl:template match="ControllerSource">    
  </xsl:template>
  
  <!-- Delete 'Control the pointer' actions -->
  <xsl:template match="ControlThePointerAction">
  </xsl:template>

  <!-- Cascade processing -->
  <xsl:template match="@*|*">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>