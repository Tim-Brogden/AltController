<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <!-- Update version -->
  <xsl:template match="profile">
    <profile version="1.4">
      <xsl:apply-templates select="node()"/>
    </profile>
  </xsl:template>

  <!-- Rename actions -->
  <xsl:template match="PointerToRepeatKeyAction">
    <RepeatKeyDirectionalAction>
      <xsl:apply-templates select="@*|node()"/>
    </RepeatKeyDirectionalAction>
  </xsl:template>
  <xsl:template match="PointerToHoldKeyAction">
    <HoldKeyAction>
      <xsl:apply-templates select="@*|node()"/>
    </HoldKeyAction>
  </xsl:template>
  <xsl:template match="PointerToReleaseKeyAction">
    <ReleaseKeyAction>
      <xsl:apply-templates select="@*|node()"/>
    </ReleaseKeyAction>
  </xsl:template>
  <xsl:template match="StopPressingThingsAction">
    <StopOngoingActionsAction>
      <xsl:apply-templates select="@*|node()"/>
    </StopOngoingActionsAction>
  </xsl:template>

  <!-- Rename region appid attribute to app -->
  <xsl:template match="region/@appid">
    <xsl:attribute name="app">
      <xsl:value-of select="."/>
    </xsl:attribute>
  </xsl:template>

  <!-- Add attributes to screen regions -->
  <xsl:template match="region">
    <region shape="Rectangle" holesize="0.5" startangle="0.0" sweepangle="90.0" bgimage="" translucency="0.5" mode="-1" page="-1">
      <xsl:apply-templates select="@*|node()"/>
    </region>
  </xsl:template>

  <!-- Cascade processing -->
  <xsl:template match="@*|*">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

</xsl:stylesheet>