<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:cdp="http://www.hl7.org.tw/EMR/CDocumentPayload/v1.0" xmlns:n1="urn:hl7-org:v3" xmlns:n2="urn:hl7-org:v3/meta/voc" xmlns:n3="http://www.w3.org/1999/xhtml" xmlns:voc="urn:hl7-org:v3/voc" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

  <!-- 變數宣告 -->
  <!-- 性別 -->
  <xsl:variable name="PatientSexValue">
    <xsl:choose>
      <xsl:when test="//n1:ClinicalDocument/n1:recordTarget/n1:patientRole/n1:patient/n1:administrativeGenderCode/@code">
        <xsl:value-of select="//n1:ClinicalDocument/n1:recordTarget/n1:patientRole/n1:patient/n1:administrativeGenderCode/@code"/>
      </xsl:when>
      <xsl:otherwise>U</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="PatientSexChineseName">
    <xsl:choose>
      <xsl:when test="$PatientSexValue='M'">男性</xsl:when>
      <xsl:when test="$PatientSexValue='F'">女性</xsl:when>
      <xsl:otherwise>未知或未設定</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <!-- 機密等級 -->
  <xsl:variable name="ConfidentialLevelValue">
    <xsl:choose>
      <xsl:when test="//n1:ClinicalDocument/n1:confidentialityCode/@code">
        <xsl:value-of select="//n1:ClinicalDocument/n1:confidentialityCode/@code"/>
      </xsl:when>
      <xsl:otherwise>U</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="ConfidentialLevelChineseName">
    <xsl:choose>
      <xsl:when test="$ConfidentialLevelValue='N'">普通</xsl:when>
      <xsl:when test="$ConfidentialLevelValue='R'">機密</xsl:when>
      <xsl:when test="$ConfidentialLevelValue='V'">極機密</xsl:when>
      <xsl:otherwise>未知或未設定</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  
  <!-- 函式(templates) -->
  <!-- Get a Name  -->
  <xsl:template name="getName">
    <xsl:param name="name"/>
    <xsl:choose>
      <xsl:when test="$name/n1:family">
        <xsl:value-of select="$name/n1:given"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="$name/n1:family"/>
        <xsl:text> </xsl:text>
        <xsl:if test="$name/n1:suffix">
          <xsl:text>, </xsl:text>
          <xsl:value-of select="$name/n1:suffix"/>
        </xsl:if>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$name"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 顯示Code + displayName old  -->
  <!--<xsl:template name="getCodeAndName">
    <xsl:param name="TargetNode"/>
    <xsl:choose>
      <xsl:when test="$TargetNode/@displayName">
        <xsl:value-of select="$TargetNode/@displayName"/>
        <xsl:if test="$TargetNode/@code">
        (<xsl:value-of select="$TargetNode/@code"/>)
        </xsl:if>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$TargetNode/@code"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>-->
  <!-- 顯示Code + displayName  -->
  <xsl:template name="getCodeAndName">
    <xsl:param name="TargetNode"/>
    <xsl:value-of select="$TargetNode/@displayName"/>
    (<xsl:value-of select="$TargetNode/@code"/>)
  </xsl:template>


  <!--時間格式轉換 參數：date：傳入之時間-->
  <xsl:template name="formatDate">
    <xsl:param name="date"/>
    <xsl:variable name="aYear" select="substring($date,1,4)"/>
    <xsl:variable name="aMonth" select="substring($date,5,2)"/>
    <xsl:variable name="aDay" select="substring($date,7,2)"/>
    <xsl:variable name="aHour" select="substring($date,9,2)"/>
    <xsl:variable name="aMin" select="substring($date,11,2)"/>
    <xsl:variable name="aSec" select="substring($date,13,2)"/>
    <xsl:if test="$aYear != ''">
      <xsl:value-of select="$aYear"/>
      <xsl:if test="$aMonth != ''">
        <xsl:text>/</xsl:text>
        <xsl:value-of select="$aMonth"/>
        <xsl:if test="$aDay != ''">
          <xsl:text>/</xsl:text>
          <xsl:value-of select="$aDay"/>
          <xsl:if test="$aHour != ''">
            <xsl:text>  </xsl:text>
            <xsl:value-of select="$aHour"/>
            <xsl:if test="$aMin != ''">
              <xsl:text>:</xsl:text>
              <xsl:value-of select="$aMin"/>
              <xsl:if test="$aSec != ''">
                <xsl:text>:</xsl:text>
                <xsl:value-of select="$aSec"/>
              </xsl:if>
            </xsl:if>
          </xsl:if>
        </xsl:if>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <!--paragraph-->
  <xsl:template match="n1:paragraph">
    <xsl:apply-templates/>
    <br />
  </xsl:template>

  <!--Tables-->
  <xsl:template match="n1:table/@*|n1:thead/@*|n1:tfoot/@*|n1:tbody/@*|n1:colgroup/@*|n1:col/@*|n1:tr/@*|n1:th/@*|n1:td/@*">
    <xsl:copy>
      <xsl:apply-templates/>
    </xsl:copy>
  </xsl:template>
  <xsl:template match="n1:table">
    <table border="1" width="100%">
      <xsl:apply-templates/>
    </table>
  </xsl:template>
  <xsl:template match="n1:thead">
    <thead>
      <xsl:apply-templates/>
    </thead>
  </xsl:template>
  <xsl:template match="n1:tfoot">
    <tfoot>
      <xsl:apply-templates/>
    </tfoot>
  </xsl:template>
  <xsl:template match="n1:tbody">
    <tbody>
      <xsl:apply-templates/>
    </tbody>
  </xsl:template>
  <xsl:template match="n1:colgroup">
    <colgroup>
      <xsl:apply-templates/>
    </colgroup>
  </xsl:template>
  <xsl:template match="n1:col">
    <col>
      <xsl:apply-templates/>
    </col>
  </xsl:template>
  <xsl:template match="n1:tr">
    <tr>
      <xsl:apply-templates/>
    </tr>
  </xsl:template>
  <xsl:template match="n1:th">
    <th>
      <xsl:apply-templates/>
    </th>
  </xsl:template>
  <xsl:template match="n1:td">
    <td>
      <xsl:apply-templates/>
    </td>
  </xsl:template>
  <!--<xsl:template match="n1:table/n1:caption">
    <span style="font-weight:bold; ">
      <xsl:apply-templates/>
    </span>
  </xsl:template>-->

  <!--New template by Phil-->
  <!-- 顯示Code + displayName  -->
  <xsl:template name="DisplayICDEntries">
    <xsl:param name="TargetSection"/>
    <xsl:choose>
      <xsl:when test="$TargetSection/n1:entry">
        <xsl:for-each select="$TargetSection/n1:entry">
          <xsl:choose>
            <xsl:when test="n1:observation/n1:code[@codeSystem='2.16.840.1.113883.6.2']">
              <xsl:value-of select="n1:observation/n1:code/@displayName"/>(<xsl:value-of select="n1:observation/n1:code/@code"/>)
              <br></br>
            </xsl:when>
          </xsl:choose>
        </xsl:for-each>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- 顯示 Body part  -->
  <xsl:template name="DisplayBodyParts">
    <xsl:param name="structuredBody"/>
    <xsl:choose>
      <xsl:when test="$structuredBody">
        <xsl:for-each select="$structuredBody/n1:component/n1:section[./n1:code[@code='55286-9' and @codeSystem='2.16.840.1.113883.6.1']]">
          <span style="padding-right:3px">
            <xsl:value-of select="./n1:text"/>
          </span>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="."/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="InsertBreaks">
    <xsl:param name="pText" />
    <xsl:choose>
      <xsl:when test="not(contains($pText, '&#xA;'))">
        <xsl:copy-of select="$pText"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="substring-before($pText, '&#xA;')"/>
        <br />
        <xsl:call-template name="InsertBreaks">
          <xsl:with-param name="pText" select="substring-after($pText, '&#xA;')"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 顯示 Text node  In paragraph-->
  <xsl:template name="DisplayTextNode">
    <xsl:param name="TextNode"/>
    <xsl:choose>
      <xsl:when test="$TextNode/n1:paragraph">
        <xsl:for-each select="$TextNode/n1:paragraph">
          <p>
            <xsl:value-of select="."/>
            <!--<xsl:call-template name="InsertBreaks">
              <xsl:with-param name="pText" select="." />
            </xsl:call-template>-->
          </p>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$TextNode"/>
        <!--<xsl:call-template name="InsertBreaks">
          <xsl:with-param name="pText" select="." />
        </xsl:call-template>-->
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 顯示 Text node  -->
  <xsl:template name="DisplayTextNodeInline">
    <xsl:param name="TextNode"/>
    <xsl:choose>
      <xsl:when test="$TextNode/n1:paragraph">
        <xsl:for-each select="$TextNode/n1:paragraph">
          <span>
            <xsl:value-of select="."/>
          </span>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$TextNode"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!--Display Images-->
  <xsl:template name="DisplayImageEntry">
    <xsl:param name="ImageEntry"/>
    <xsl:choose>
      <xsl:when test="$ImageEntry/n1:observationMedia">
        <xsl:for-each select="$ImageEntry/n1:observationMedia">
          <img>
            <xsl:attribute name="src">
              data:<xsl:value-of select="./n1:value/@mediaType"/>;base64,<xsl:value-of select="./n1:value"/>
            </xsl:attribute>
          </img>
        </xsl:for-each>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  
</xsl:stylesheet>