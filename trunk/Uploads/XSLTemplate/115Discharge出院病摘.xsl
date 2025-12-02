<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:cdp="http://www.hl7.org.tw/EMR/CDocumentPayload/v1.0" xmlns:n1="urn:hl7-org:v3" xmlns:n2="urn:hl7-org:v3/meta/voc" xmlns:n3="http://www.w3.org/1999/xhtml" xmlns:voc="urn:hl7-org:v3/voc" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
<xsl:include href="Common.xsl"/>

<xsl:variable name="version115">
  <xsl:text>115:2011-06-03-00</xsl:text>
</xsl:variable>

<xsl:template match="/">
 <top>
   <xsl:apply-templates/>
 </top>
</xsl:template>

<xsl:template match="//n1:ClinicalDocument" mode="Discharge">
  <xsl:variable name="title">
    <xsl:choose>
         <xsl:when test="//n1:ClinicalDocument/n1:title">
             <xsl:value-of select="//n1:ClinicalDocument/n1:title"/>
         </xsl:when>
         <xsl:otherwise>出院病摘</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <div class="CDA-ReportWarpper">
    <table class="CDA-ReportTable">
      <tr clas="CDA-ReportTable_FirstRow">
        <td class="CDA-ReportTable_ColumnOne"></td>
        <td class="CDA-ReportTable_ColumnTwo"></td>
        <td class="CDA-ReportTable_ColumnThree"></td>
        <td class="CDA-ReportTable_ColumnFour"></td>
      </tr>
      <tr>
        <td colspan="4" class="CDA-ReportTitle">
          <xsl:value-of select="$title"/>
        </td>
      </tr>
      <tr>
        <td colspan="4" class="CDA-ReportSecretRank">
          機密等級：<xsl:value-of select="$ConfidentialLevelChineseName"/>
        </td>
      </tr>
      <tr>
        <th>
          <xsl:text>醫療機構代碼：</xsl:text>
        </th>
        <td>
          <xsl:value-of select="n1:custodian/n1:assignedCustodian/n1:representedCustodianOrganization/n1:id[@root='2.16.886.101.20003.20014']/@extension"/>
        </td>
        <th>
          <xsl:text>醫療機構名稱：</xsl:text>
        </th>
        <td>
          <xsl:value-of select="n1:custodian/n1:assignedCustodian/n1:representedCustodianOrganization/n1:name"/>
        </td>
      </tr>
      <!--Patient Info-->
      <tr>
        <td colspan="4" class="CDA-ReportDivBar">病人基本資料</td>
      </tr>
      <tr>
        <th>
          <xsl:text>病歷號碼：</xsl:text>
        </th>
        <td>
          <xsl:value-of select="n1:recordTarget/n1:patientRole/n1:id/@extension"/>
        </td>
      </tr>
      <tr>
        <th>
          <xsl:text>病人姓名：</xsl:text>
        </th>
        <td>
          <xsl:call-template name="getName">
            <xsl:with-param name="name" select="n1:recordTarget/n1:patientRole/n1:patient/n1:name"/>
          </xsl:call-template>
        </td>
        <th>
          <xsl:text>病人性別：</xsl:text>
        </th>
        <td>
          <xsl:value-of select="$PatientSexChineseName"/>(<xsl:value-of select="$PatientSexValue"/>)
        </td>
      </tr>
      <tr>
        <th>
          <xsl:text>身份證號：</xsl:text>
        </th>
        <td>
          <xsl:value-of select="n1:recordTarget/n1:patientRole/n1:patient/n1:id/@extension"/>
        </td>
        <th>
          <xsl:text>出生日期：</xsl:text>
        </th>
        <td>
          <xsl:call-template name="formatDate">
            <xsl:with-param name="date" select="n1:recordTarget/n1:patientRole/n1:patient/n1:birthTime/@value" />
          </xsl:call-template>
        </td>
      </tr>
      <!--Study Info-->
      <tr>
        <td colspan="4" class="CDA-ReportDivBar">報告資訊</td>
      </tr>
      <tr>
        <th>
          <xsl:text>住院日期：</xsl:text>
        </th>
        <td>
          <xsl:call-template name="formatDate">
            <xsl:with-param name="date" select="n1:componentOf/n1:encompassingEncounter/n1:effectiveTime/n1:low/@value" />
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <th>
          <xsl:text>出院日期：</xsl:text>
        </th>
        <td>
          <xsl:call-template name="formatDate">
            <xsl:with-param name="date" select="n1:componentOf/n1:encompassingEncounter/n1:effectiveTime/n1:high/@value" />
          </xsl:call-template>
        </td>
        <th>
          <xsl:text>出院科別：</xsl:text>
        </th>
        <td>
          <xsl:value-of select="n1:componentOf/n1:encompassingEncounter/n1:location/n1:healthCareFacility/n1:location/n1:name" />
        </td>
      </tr>
      <tr>
        <th>
          <xsl:text>創建報告時間：</xsl:text>
        </th>
        <td>
          <xsl:call-template name="formatDate">
            <xsl:with-param name="date" select="n1:effectiveTime/@value" />
          </xsl:call-template>
        </td>
        <th>
          <xsl:text>創建報告醫師：</xsl:text>
        </th>
        <td>
          <xsl:call-template name="getName">
            <xsl:with-param name="name" select="n1:author/n1:assignedAuthor/n1:assignedPerson/n1:name"/>
          </xsl:call-template>
        </td>
      </tr>
      <xsl:if test ="n1:legalAuthenticator">
        <tr>
          <th>
            <xsl:text>認證報告時間：</xsl:text>
          </th>
          <td>
            <xsl:call-template name="formatDate">
              <xsl:with-param name="date" select="n1:legalAuthenticator/n1:time/@value" />
            </xsl:call-template>
          </td>
          <th>
            <xsl:text>認證報告醫師：</xsl:text>
          </th>
          <td>
            <xsl:call-template name="getName">
              <xsl:with-param name="name" select="n1:legalAuthenticator/n1:assignedEntity/n1:assignedPerson/n1:name"/>
            </xsl:call-template>
          </td>
        </tr>
      </xsl:if>
      <!--Report Content-->
      <tr>
        <td colspan="4" class="CDA-ReportDivBar">報告內容</td>
      </tr>
      <tr>
        <th>住院臆斷：</th>
        <td colspan="3">
          <xsl:call-template name="DisplayTextNode">
            <xsl:with-param name="TextNode" select="n1:component/n1:structuredBody/n1:component/n1:section/n1:text[../n1:code[@code='46241-6' and @codeSystem='2.16.840.1.113883.6.1']]"/>
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <th>出院診斷：</th>
        <td colspan="3">
          <xsl:call-template name="DisplayICDEntries">
            <xsl:with-param name="TargetSection" select="n1:component/n1:structuredBody/n1:component/n1:section[./n1:code[@code='11535-2' and @codeSystem='2.16.840.1.113883.6.1']]"/>
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <th>癌症期別：</th>
        <td colspan="3">
          <xsl:call-template name="DisplayTextNodeInline">
            <xsl:with-param name="TextNode" select="n1:component/n1:structuredBody/n1:component/n1:section/n1:text[../n1:code[@code='22037-6' and @codeSystem='2.16.840.1.113883.6.1']]"/>
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <th>主訴：</th>
        <td colspan="3">
          <xsl:call-template name="DisplayTextNode">
            <xsl:with-param name="TextNode" select="n1:component/n1:structuredBody/n1:component/n1:section/n1:text[../n1:code[@code='10154-3' and @codeSystem='2.16.840.1.113883.6.1']]"/>
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <th>病史：</th>
        <td colspan="3">
          <xsl:call-template name="DisplayTextNode">
            <xsl:with-param name="TextNode" select="n1:component/n1:structuredBody/n1:component/n1:section/n1:text[../n1:code[@code='10164-2' and @codeSystem='2.16.840.1.113883.6.1']]"/>
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <th>理學檢查：</th>
        <td colspan="3">
          <xsl:call-template name="DisplayTextNode">
            <xsl:with-param name="TextNode" select="n1:component/n1:structuredBody/n1:component/n1:section/n1:text[../n1:code[@code='29545-1' and @codeSystem='2.16.840.1.113883.6.1']]"/>
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <th>檢驗：</th>
        <td colspan="3">
          <xsl:call-template name="DisplayTextNode">
            <xsl:with-param name="TextNode" select="n1:component/n1:structuredBody/n1:component/n1:section/n1:text[../n1:code[@code='30954-2' and @codeSystem='2.16.840.1.113883.6.1']]"/>
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <th>特殊檢查：</th>
        <td colspan="3">
          <xsl:call-template name="DisplayTextNode">
            <xsl:with-param name="TextNode" select="n1:component/n1:structuredBody/n1:component/n1:section/n1:text[../n1:code[@code='19146-0' and @codeSystem='2.16.840.1.113883.6.1']]"/>
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <th>醫療影像檢查：</th>
        <td colspan="3">
          <xsl:call-template name="DisplayTextNode">
            <xsl:with-param name="TextNode" select="n1:component/n1:structuredBody/n1:component/n1:section/n1:text[../n1:code[@code='19005-8' and @codeSystem='2.16.840.1.113883.6.1']]"/>
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <th>手術：</th>
        <td colspan="3">
          <xsl:call-template name="DisplayTextNode">
            <xsl:with-param name="TextNode" select="n1:component/n1:structuredBody/n1:component/n1:section/n1:text[../n1:code[@code='8724-7' and @codeSystem='2.16.840.1.113883.6.1']]"/>
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <th>病理：</th>
        <td colspan="3">
          <xsl:call-template name="DisplayTextNode">
            <xsl:with-param name="TextNode" select="n1:component/n1:structuredBody/n1:component/n1:section/n1:text[../n1:code[@code='22034-3' and @codeSystem='2.16.840.1.113883.6.1']]"/>
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <th>治療經過：</th>
        <td colspan="3">
          <xsl:call-template name="DisplayTextNode">
            <xsl:with-param name="TextNode" select="n1:component/n1:structuredBody/n1:component/n1:section/n1:text[../n1:code[@code='8648-8' and @codeSystem='2.16.840.1.113883.6.1']]"/>
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <th>合併症與併發症：</th>
        <td colspan="3">
          <xsl:call-template name="DisplayTextNode">
            <xsl:with-param name="TextNode" select="n1:component/n1:structuredBody/n1:component/n1:section/n1:text[../n1:code[@code='55109-3' and @codeSystem='2.16.840.1.113883.6.1']]"/>
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <th>出院指示：</th>
        <td colspan="3">
          <xsl:call-template name="DisplayTextNode">
            <xsl:with-param name="TextNode" select="n1:component/n1:structuredBody/n1:component/n1:section/n1:text[../n1:code[@code='8653-8' and @codeSystem='2.16.840.1.113883.6.1']]"/>
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <th>出院狀況：</th>
        <td colspan="3">
          <xsl:call-template name="DisplayTextNode">
            <xsl:with-param name="TextNode" select="n1:component/n1:structuredBody/n1:component/n1:section/n1:text[../n1:code[@code='42345-9' and @codeSystem='2.16.840.1.113883.6.1']]"/>
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <th></th>
        <td colspan="3"></td>
      </tr>
      <xsl:choose>
        <xsl:when test="n1:component/n1:structuredBody/n1:component/n1:section/n1:entry[../n1:code[@code='19005-8' and @codeSystem='2.16.840.1.113883.6.1']]/n1:observationMedia">
          <tr>
            <th>影像：</th>
            <td colspan="3">
              <xsl:for-each select="n1:component/n1:structuredBody/n1:component/n1:section/n1:entry[../n1:code[@code='19005-8' and @codeSystem='2.16.840.1.113883.6.1']]/n1:observationMedia">
                <img>
                  <xsl:attribute name="src">
                    data:<xsl:value-of select="./n1:value/@mediaType"/>;base64,<xsl:value-of select="./n1:value"/>
                  </xsl:attribute>
                </img>
              </xsl:for-each>
            </td>
          </tr>
        </xsl:when>
      </xsl:choose>
    </table>
  </div>
</xsl:template>

</xsl:stylesheet>
