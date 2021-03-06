<?xml version="1.0"?>

<!--
Copyright (c) 2019 Sanich.
Originally made available on https://cdrpro.ru
-->

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:frmwrk="Corel Framework Data">
  <xsl:output method="xml" encoding="UTF-8" indent="yes"/>
  
  <frmwrk:uiconfig>
    <frmwrk:applicationInfo userConfiguration="true" />
  </frmwrk:uiconfig>
  
  <xsl:template match="node()|@*">
    <xsl:copy>
      <xsl:apply-templates select="node()|@*"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="uiConfig/items">
    <xsl:copy>
      <xsl:apply-templates select="node()|@*"/>
      
      <itemData guid="33c5af71-a99c-41d8-916d-92fae5d5e489"
      			dynamicCommand="CdrToolsEx.CdrToolsExDocker.Docker"
                dynamicCategory="2cc24a3e-fe24-4708-9a74-9c75406eebcd"
                userCaption="CdrToolsEx"
                userToolTip="Show or hide CdrToolsEx docker">
      		<userSmallBitmap xmlns:dt="urn:schemas-microsoft-com:datatypes" dt:dt="bin.base64">
                //8BABAAV0NtblVJX1VJSXRlbUJtcAAAAAAAAAAAKAAAAAAEAAAAAQAAKAAAABAAAAAQAAAA
				AQAIAAAAAAAAAQAAAAAAAAAAAAAAAQAAAAEAAAAAAAAAAIAAAIAAAACAgACAAAAAgACAAICA
				AADAwMAAwNzAAPDKpgDw+/8ApKCgAICAgAAAAP8AAP8AAAD//wD/AAAA/wD/AP//AAD///8A
				AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
				AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
				AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
				AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
				AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
				AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
				AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
				AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
				AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
				AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
				AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
				AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
				AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
				AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
				AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
				AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
				AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
				AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAMAAwHBwcHBwcHBwcMAQwHAAwADAcHBwcHBwcM
				AQcBBwwADAAMBwcHBwcMAQsBDAcHDAAMAAwHBwcMAQsBDAcHBwcMAAwADAcMAQsNDAcHBwcH
				BwwAAAAMAQsNDAcHBwcHBwcHDAABAQwNDAcHBwcHBwcHBwcMAQcBCwcHBwwMCwcHBwcHDAsN
				DAwHBwsMCgwHBwcHDAcMBwwHDAwMBwwLBwcHDAcMBwcHDAgHCAwLBwcHDAcMBwcHBwwIBwgM
				BwcMDAcMBwcHBwsLCAgHCwcHDAoLBwcHBwcLBwoLCwcHBwsLCwcHBwcLCwsLCwcHBwcHBwcH
				BwsLCwsHBwcHBwcH8PDwAKCgoADw8PAAAAAAAA==
            </userSmallBitmap>
        </itemData>
    </xsl:copy>
  </xsl:template>
  
</xsl:stylesheet>