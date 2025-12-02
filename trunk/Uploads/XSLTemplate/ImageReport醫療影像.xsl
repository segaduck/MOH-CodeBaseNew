<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:cdp="http://www.hl7.org.tw/EMR/CDocumentPayload/v1.0" xmlns:n1="urn:hl7-org:v3" xmlns:n2="urn:hl7-org:v3/meta/voc" xmlns:n3="http://www.w3.org/1999/xhtml" xmlns:voc="urn:hl7-org:v3/voc" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
<xsl:include href="Common.xsl"/>

<xsl:variable name="version116">
  <xsl:text>116:2011-06-03-00</xsl:text>
</xsl:variable>

<xsl:template match="/">
 <top>
   <xsl:apply-templates/>
 </top>
</xsl:template>

<xsl:template match="//n1:ClinicalDocument" mode="ImageReport">
  <xsl:variable name="title">
    <xsl:choose>
         <xsl:when test="//n1:ClinicalDocument/n1:title">
             <xsl:value-of select="//n1:ClinicalDocument/n1:title"/>
         </xsl:when>
         <xsl:otherwise>影像報告</xsl:otherwise>
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
        <td colspan="4" class="CDA-ReportDivBar">檢查資訊</td>
      </tr>
      <tr>
        <th>
          <xsl:text>醫令代碼：</xsl:text>
        </th>
        <td colspan="3">
          <xsl:call-template name="getCodeAndName">
            <xsl:with-param name="TargetNode" select="n1:code/n1:translation" />
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <th>
          <xsl:text>檢查單號：</xsl:text>
        </th>
        <td>
          <xsl:value-of select="n1:inFulfillmentOf/n1:order/n1:id/@extension"/>
        </td>
        <th>
          <xsl:text>醫囑日期：</xsl:text>
        </th>
        <td>
          <xsl:call-template name="formatDate">
            <xsl:with-param name="date" select="//n1:ClinicalDocument/n1:participant/n1:time/@value" />
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <th>
          <xsl:text>門診或住院時間：</xsl:text>
        </th>
        <td>
          <xsl:call-template name="formatDate">
            <xsl:with-param name="date" select="n1:componentOf/n1:encompassingEncounter/n1:effectiveTime/@value" />
          </xsl:call-template>
        </td>
        <th>
          <xsl:text>主治醫師：</xsl:text>
        </th>
        <td>
          <xsl:call-template name="getName">
            <xsl:with-param name="name" select="n1:componentOf/n1:encompassingEncounter/n1:encounterParticipant/n1:assignedEntity/n1:assignedPerson/n1:name"/>
          </xsl:call-template>
          -
          <xsl:value-of select="n1:componentOf/n1:encompassingEncounter/n1:location/n1:healthCareFacility/n1:location/n1:name"/>
        </td>
      </tr>
      <tr>
        <th>
          <xsl:text>檢查時間：</xsl:text>
        </th>
        <td>
          <xsl:variable name="StudyDateTime">
            <xsl:choose>
              <xsl:when test="//n1:ClinicalDocument/n1:documentationOf/n1:serviceEvent/n1:effectiveTime/@value">
                <xsl:value-of select="//n1:ClinicalDocument/n1:documentationOf/n1:serviceEvent/n1:effectiveTime/@value"/>
              </xsl:when>
              <xsl:when test="//n1:ClinicalDocument/n1:documentationOf/n1:serviceEvent/n1:effectiveTime/n1:low/@value">
                <xsl:value-of select="//n1:ClinicalDocument/n1:documentationOf/n1:serviceEvent/n1:effectiveTime/n1:low/@value"/>
              </xsl:when>
            </xsl:choose>
          </xsl:variable>
          <xsl:call-template name="formatDate">
            <xsl:with-param name="date" select="$StudyDateTime" />
          </xsl:call-template>
        </td>
        <th>
          <xsl:text>檢查醫師：</xsl:text>
        </th>
        <td>
          <xsl:call-template name="getName">
            <xsl:with-param name="name" select="n1:documentationOf/n1:serviceEvent/n1:performer/n1:assignedEntity/n1:assignedPerson/n1:name"/>
          </xsl:call-template>
          -
          <xsl:value-of select="n1:documentationOf/n1:serviceEvent/n1:performer/n1:assignedEntity/n1:representedOrganization/n1:name"/>
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
      <!--Report Content-->
      <tr>
        <td colspan="4" class="CDA-ReportDivBar">報告內容</td>
      </tr>
      <tr>
        <th>病史：</th>
        <td colspan="3">
          <xsl:value-of select="n1:component/n1:structuredBody/n1:component/n1:section/n1:text[../n1:code[@code='10164-2' and @codeSystem='2.16.840.1.113883.6.1']]"/>
        </td>
      </tr>
      <tr>
        <th>主訴：</th>
        <td colspan="3">
          <xsl:value-of select="n1:component/n1:structuredBody/n1:component/n1:section/n1:component[../n1:code[@code='10164-2' and @codeSystem='2.16.840.1.113883.6.1']]/n1:section/n1:text[../n1:code[@code='10154-3' and @codeSystem='2.16.840.1.113883.6.1']]"/>
        </td>
      </tr>
      <tr>
        <th>適應症：</th>
        <td colspan="3">
          <xsl:value-of select="n1:component/n1:structuredBody/n1:component/n1:section/n1:component[../n1:code[@code='10164-2' and @codeSystem='2.16.840.1.113883.6.1']]/n1:section/n1:text[../n1:code[@code='19777-2' and @codeSystem='2.16.840.1.113883.6.1']]"/>
        </td>
      </tr>
      <tr>
        <th>疾病診斷：</th>
        <td colspan="3">
          <xsl:call-template name="DisplayICDEntries">
            <xsl:with-param name="TargetSection" select="n1:component/n1:structuredBody/n1:component/n1:section[./n1:code[@code='52797-8' and @codeSystem='2.16.840.1.113883.6.1']]"/>
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <th>診斷說明：</th>
        <td colspan="3">
          <xsl:value-of select="n1:component/n1:structuredBody/n1:component/n1:section/n1:text[../n1:code[@code='52797-8' and @codeSystem='2.16.840.1.113883.6.1']]"/>
        </td>
      </tr>
      <tr>
        <th>診斷時間：</th>
        <td colspan="3">
          <xsl:call-template name="formatDate">
            <xsl:with-param name="date" select="n1:component/n1:structuredBody/n1:component/n1:section[./n1:code[@code='52797-8' and @codeSystem='2.16.840.1.113883.6.1']]/n1:entry/n1:observation/n1:effectiveTime/@value" />
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <th>部位：</th>
        <td colspan="3">
          <xsl:call-template name="DisplayBodyParts">
            <xsl:with-param name="structuredBody" select="n1:component/n1:structuredBody"/>
          </xsl:call-template>
        </td>
      </tr>
      <tr>
        <th>張數：</th>
        <td colspan="3">
          <xsl:value-of select="n1:component/n1:structuredBody/n1:component/n1:section/n1:entry[../n1:code[@code='33034-0' and @codeSystem='2.16.840.1.113883.6.1']]/n1:observation/n1:value/@value"/>
        </td>
      </tr>
      <tr>
        <th>影像發現：</th>
        <td colspan="3">
          <xsl:value-of select="n1:component/n1:structuredBody/n1:component/n1:section/n1:text[../n1:code[@code='18782-3' and @codeSystem='2.16.840.1.113883.6.1']]"/>
        </td>
      </tr>
      <tr>
        <th>結果：</th>
        <td colspan="3">
          <xsl:value-of select="n1:component/n1:structuredBody/n1:component/n1:section/n1:text[../n1:code[@code='11515-4' and @codeSystem='2.16.840.1.113883.6.1']]"/>
        </td>
      </tr>
      <tr>
        <th>觀察：</th>
        <td colspan="3">
          <xsl:value-of select="n1:component/n1:structuredBody/n1:component/n1:section/n1:component[../n1:code[@code='11515-4' and @codeSystem='2.16.840.1.113883.6.1']]/n1:section/n1:text[../n1:code[@code='29545-1' and @codeSystem='2.16.840.1.113883.6.1']]"/>
        </td>
      </tr>
      <tr>
        <th>臆斷：</th>
        <td colspan="3">
          <xsl:value-of select="n1:component/n1:structuredBody/n1:component/n1:section/n1:component[../n1:code[@code='11515-4' and @codeSystem='2.16.840.1.113883.6.1']]/n1:section/n1:text[../n1:code[@code='44833-2' and @codeSystem='2.16.840.1.113883.6.1']]"/>
        </td>
      </tr>
      <tr>
        <th>註記：</th>
        <td colspan="3">
          <xsl:value-of select="n1:component/n1:structuredBody/n1:component/n1:section/n1:component[../n1:code[@code='11515-4' and @codeSystem='2.16.840.1.113883.6.1']]/n1:section/n1:text[../n1:code[@code='51855-5' and @codeSystem='2.16.840.1.113883.6.1']]"/>
        </td>
      </tr>
      <tr>
        <th>建議：</th>
        <td colspan="3">
          <xsl:value-of select="n1:component/n1:structuredBody/n1:component/n1:section/n1:text[../n1:code[@code='18783-1' and @codeSystem='2.16.840.1.113883.6.1']]"/>
        </td>
      </tr>
    </table>
  </div>
</xsl:template>

</xsl:stylesheet>
