<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <!-- Update version -->
  <xsl:template match="profile">
    <profile version="0.6">
      <xsl:apply-templates select="node()"/>
    </profile>
  </xsl:template>

  <!-- Add attributes to region -->
  <xsl:template match="region">
    <region appid="1" colour="LightGray">
      <xsl:apply-templates select="@*|node()"/>
    </region>
  </xsl:template>

  <!-- Add executionmode attribute to actionlist -->
  <xsl:template match="actionlist">
    <actionlist executionmode="Series">
      <xsl:apply-templates select="@*|node()"/>
    </actionlist>  
  </xsl:template>
  
  <!-- Rename keystate attribute as eventdata -->
  <xsl:template match="actionlist/@keystate">
    <xsl:attribute name="eventdata">
      <xsl:value-of select="."/>
    </xsl:attribute>
  </xsl:template>
  <xsl:template match="param/@keystate">
    <xsl:attribute name="eventdata">
      <xsl:value-of select="."/>
    </xsl:attribute>
  </xsl:template>

  <!-- Change param datatypes from Pixel to Float -->
  <xsl:template match="param/@datatype['Pixel']">
    <xsl:attribute name="datatype">Float</xsl:attribute>
  </xsl:template>

  <!-- Cascade processing -->
  <xsl:template match="@*|*">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>