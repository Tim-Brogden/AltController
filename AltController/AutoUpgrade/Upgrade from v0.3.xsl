<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <!-- Update version -->
  <xsl:template match="profile">
    <profile version="0.4">
      <xsl:apply-templates select="node()"/>
    </profile>
  </xsl:template>

  <!-- PressKeyAction -->
  <xsl:template match="PressKeyAction">
    <RepeatKeyAction>
      <xsl:apply-templates select="@*|node()"/>
    </RepeatKeyAction>
  </xsl:template>

  <!-- PointerToKeyAction -->
  <xsl:template match="PointerToKeyAction">
    <xsl:if test="@direction!='None'">
      <PointerToRepeatKeyAction regionname="" sensitivity="1.0">
        <xsl:apply-templates select="@*|node()"/>
      </PointerToRepeatKeyAction>
    </xsl:if>
    <xsl:if test="@direction='None'">
      <PointerToHoldKeyAction autorelease="true" regionname="">
        <xsl:attribute name="key">
          <xsl:value-of select="@key"/>
        </xsl:attribute>
        <xsl:attribute name="regionid">
          <xsl:value-of select="@screenregion"/>
        </xsl:attribute>
        <xsl:apply-templates select="node()"/>
      </PointerToHoldKeyAction>
    </xsl:if>
  </xsl:template>
  <xsl:template match="PointerToKeyAction/@keytocancel"/>
  <xsl:template match="PointerToKeyAction/@screenregion">
    <xsl:attribute name="regionid">
      <xsl:value-of select="."/>
    </xsl:attribute>
  </xsl:template>
  <xsl:template match="PointerToKeyAction/@maxduration">
    <xsl:attribute name="timetomax">
      <xsl:value-of select="."/>
    </xsl:attribute>
  </xsl:template>
  <xsl:template match="PointerToKeyAction/@autocancel">
    <xsl:attribute name="timetomin">
      <xsl:value-of select="."/>
    </xsl:attribute>
  </xsl:template>

  <!-- Cascade processing -->
  <xsl:template match="@*|*">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>