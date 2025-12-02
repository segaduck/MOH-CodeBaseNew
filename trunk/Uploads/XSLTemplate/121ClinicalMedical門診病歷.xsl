<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:cdp="http://www.hl7.org.tw/EMR/CDocumentPayload/v1.0" xmlns:n1="urn:hl7-org:v3" xmlns:n2="urn:hl7-org:v3/meta/voc" xmlns:n3="http://www.w3.org/1999/xhtml" xmlns:voc="urn:hl7-org:v3/voc" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <xsl:include href="Common.xsl"/>

  <xsl:variable name="version121">
    <xsl:text>121:2013-06-30-00</xsl:text>
  </xsl:variable>

<xsl:template match="/">
 <top>
   <xsl:apply-templates/>
 </top>
</xsl:template>

  <xsl:template match="//n1:ClinicalDocument" mode="ClinicalMedical">
    <xsl:variable name="title">
      <xsl:choose>
        <xsl:when test="//n1:ClinicalDocument/n1:title">
          <xsl:value-of select="//n1:ClinicalDocument/n1:title"/>
        </xsl:when>
        <xsl:otherwise>門診病歷</xsl:otherwise>
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
        <!--病人生活史-->
        <xsl:variable name="secSocialHistory" select="n1:component/n1:structuredBody/n1:component/n1:section[./n1:code[@code='29762-2' and @codeSystem='2.16.840.1.113883.6.1']]" />
        <tr>
          <td colspan="4" class="CDA-ReportDivBar">病人生活史</td>
        </tr>
        <tr>
          <th>
            <xsl:text>就診年齡：</xsl:text>
          </th>
          <td>
            <xsl:call-template name="DisplayTextNode">
              <xsl:with-param name="TextNode" select="$secSocialHistory/n1:component/n1:section[./n1:code[@code='29553-5' and @codeSystem='2.16.840.1.113883.6.1']]/n1:text"/>
            </xsl:call-template>
          </td>
        </tr>
        <tr>
          <th>
            <xsl:text>職業：</xsl:text>
          </th>
          <td>
            <xsl:call-template name="DisplayTextNode">
              <xsl:with-param name="TextNode" select="$secSocialHistory/n1:component/n1:section[./n1:code[@code='21847-9' and @codeSystem='2.16.840.1.113883.6.1']]/n1:text"/>
            </xsl:call-template>
          </td>
        </tr>
        <tr>
          <th>
            <xsl:text>就醫身分別：</xsl:text>
          </th>
          <td>
            <xsl:call-template name="DisplayTextNode">
              <xsl:with-param name="TextNode" select="$secSocialHistory/n1:component/n1:section[./n1:code[@code='63513-6' and @codeSystem='2.16.840.1.113883.6.1']]/n1:text"/>
            </xsl:call-template>
          </td>
        </tr>
        <tr>
          <th>
            <xsl:text>門診日期：</xsl:text>
          </th>
          <td>
            <xsl:call-template name="formatDate">
              <xsl:with-param name="date" select="n1:componentOf/n1:encompassingEncounter/n1:effectiveTime/@value" />
            </xsl:call-template>
          </td>
          <th>
            <xsl:text>科別：</xsl:text>
          </th>
          <td>
            <xsl:value-of select="n1:componentOf/n1:encompassingEncounter/n1:location/n1:healthCareFacility/n1:location/n1:name" />
          </td>
        </tr>
        <!--實驗室檢查紀錄-->
        <xsl:variable name="secLabTestResult" select="n1:component/n1:structuredBody/n1:component/n1:section[./n1:code[@code='19146-0' and @codeSystem='2.16.840.1.113883.6.1']]" />
        <tr>
          <td colspan="4" class="CDA-ReportDivBar">實驗室檢查紀錄</td>
        </tr>
        <tr>
          <th>
            <xsl:text>血型：</xsl:text>
          </th>
          <td>
            <xsl:call-template name="DisplayTextNode">
              <xsl:with-param name="TextNode" select="$secLabTestResult/n1:component/n1:section[./n1:code[@code='883-9' and @codeSystem='2.16.840.1.113883.6.1']]/n1:text"/>
            </xsl:call-template>
          </td>
        </tr>
        <tr>
          <th>
            <xsl:text>D抗原性：</xsl:text>
          </th>
          <td>
            <xsl:call-template name="DisplayTextNode">
              <xsl:with-param name="TextNode" select="$secLabTestResult/n1:component/n1:section[./n1:code[@code='10331-7' and @codeSystem='2.16.840.1.113883.6.1']]/n1:text"/>
            </xsl:call-template>
          </td>
        </tr>
        <!--重大傷病-->
        <xsl:variable name="secHistoryMajorIllnessAndInjuries" select="n1:component/n1:structuredBody/n1:component/n1:section[./n1:code[@code='11338-1' and @codeSystem='2.16.840.1.113883.6.1']]" />
        <tr>
          <th>
            <xsl:text>重大傷病：</xsl:text>
          </th>
          <td colspan="3">
            <xsl:for-each select="$secHistoryMajorIllnessAndInjuries/n1:entry">
              <xsl:choose>
                <xsl:when test="n1:observation[@negationInd='true']">
                  NA <br />
                </xsl:when>
                <xsl:otherwise>
                  (<xsl:value-of select="n1:observation/n1:code/@code" />) <xsl:value-of select="n1:observation/n1:code/@displayName" /> <br />
                </xsl:otherwise>
              </xsl:choose>
            </xsl:for-each>
          </td>
        </tr>
        <!--過敏史-->
        <tr>
          <th>
            <xsl:text>過敏史：</xsl:text>
          </th>
          <td colspan="3">
            <xsl:call-template name="DisplayTextNode">
              <xsl:with-param name="TextNode" select="n1:component/n1:structuredBody/n1:component/n1:section/n1:text[../n1:code[@code='10155-0' and @codeSystem='2.16.840.1.113883.6.1']]"/>
            </xsl:call-template>
          </td>
        </tr>
        <!--診斷-->
        <xsl:variable name="secDiagnosis" select="n1:component/n1:structuredBody/n1:component/n1:section[./n1:code[@code='29548-5' and @codeSystem='2.16.840.1.113883.6.1']]" />
        <tr>
          <th>
            <xsl:text>診斷：</xsl:text>
          </th>
          <td colspan="3">
            <xsl:for-each select="$secDiagnosis/n1:entry">
              (<xsl:value-of select="n1:observation/n1:code/@code" />) <xsl:value-of select="n1:observation/n1:code/@displayName" /> <br />
              <xsl:if test ="n1:observation/n1:text">
                備註：<xsl:value-of select="n1:observation/n1:text" /> <br />
              </xsl:if>
            </xsl:for-each>
          </td>
        </tr>
        <!--病情摘要-->
        <xsl:variable name="secReturnVisitConditions" select="n1:component/n1:structuredBody/n1:component/n1:section[./n1:code[@code='19824-2' and @codeSystem='2.16.840.1.113883.6.1']]" />
        <tr>
          <td colspan="4" class="CDA-ReportDivBar">病情摘要</td>
        </tr>
        <tr>
          <th>
            <xsl:text>主觀描述：</xsl:text>
          </th>
          <td colspan="3">
            <xsl:call-template name="DisplayTextNode">
              <xsl:with-param name="TextNode" select="$secReturnVisitConditions/n1:component/n1:section[./n1:code[@code='61150-9' and @codeSystem='2.16.840.1.113883.6.1']]/n1:text"/>
            </xsl:call-template>
          </td>
        </tr>
        <tr>
          <th>
            <xsl:text>客觀描述：</xsl:text>
          </th>
          <td colspan="3">
            <xsl:call-template name="DisplayTextNode">
              <xsl:with-param name="TextNode" select="$secReturnVisitConditions/n1:component/n1:section[./n1:code[@code='61149-1' and @codeSystem='2.16.840.1.113883.6.1']]/n1:text"/>
            </xsl:call-template>
          </td>
        </tr>
        <tr>
          <th>
            <xsl:text>評估：</xsl:text>
          </th>
          <td colspan="3">
            <xsl:call-template name="DisplayTextNode">
              <xsl:with-param name="TextNode" select="$secReturnVisitConditions/n1:component/n1:section[./n1:code[@code='11494-2' and @codeSystem='2.16.840.1.113883.6.1']]/n1:text"/>
            </xsl:call-template>
          </td>
        </tr>
      </table>
      <!--處置項目-->
      <xsl:variable name="secProcedure" select="n1:component/n1:structuredBody/n1:component/n1:section[./n1:code[@code='29554-3' and @codeSystem='2.16.840.1.113883.6.1']]" />
      <p class="CDA-ReportDivBar">處置項目</p>
      <xsl:call-template name="ShowProcedureItems">
        <xsl:with-param name="secProcedure" select="$secProcedure" />
      </xsl:call-template>
      <!--處方-->
      <xsl:variable name="secPrescribe" select="n1:component/n1:structuredBody/n1:component/n1:section[./n1:code[@code='29551-9' and @codeSystem='2.16.840.1.113883.6.1']]" />
      <p class="CDA-ReportDivBar">處方</p>
       <xsl:call-template name="ShowPrescribeItems">
        <xsl:with-param name="secPrescribe" select="$secPrescribe" />
      </xsl:call-template>
    </div>
  </xsl:template>
  <xsl:template name="ShowProcedureItems">
    <xsl:param name="secProcedure"/>
    <table class="CDA-ReportItemListTable">
      <tr>
        <th>項次</th>
        <th>處置代碼</th>
        <th>處置名稱</th>
        <th>頻率</th>
        <th>數量(單位)</th>
        <th>部位</th>
        <th>註記</th>
		<th>自費註記</th>
      </tr>
      <xsl:for-each select="$secProcedure/n1:entry">
        <xsl:choose>
          <xsl:when test="n1:procedure[@negationInd='true']">
            <tr>
              <td colspan="8">當次門診無開立處置紀錄</td>
            </tr>
          </xsl:when>
          <xsl:otherwise>
            <tr>
              <td>
                <xsl:value-of select="n1:procedure/n1:id/@extension" />
              </td>
              <td>
                <xsl:value-of select="n1:procedure/n1:code/@code" />
              </td>
              <td>
                <xsl:value-of select="n1:procedure/n1:code/@displayName" />
              </td>
              <td>
                <xsl:value-of select="n1:procedure/n1:precondition/n1:criterion/n1:text" />
              </td>
              <td>
                <xsl:value-of select="n1:procedure/n1:precondition/n1:criterion/n1:value/@value" />
                (<xsl:value-of select="n1:procedure/n1:precondition/n1:criterion/n1:value/@unit" />)
              </td>
              <td>
                <xsl:value-of select="n1:procedure/n1:targetSiteCode/@displayName" />
              </td>
              <td>
                <xsl:value-of select="n1:procedure/n1:text" />
              </td>
			  <td>
                <xsl:value-of select="n1:procedure/n1:entryRelationship/n1:act/n1:text" />
              </td>
            </tr>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
    </table>
  </xsl:template>
  <xsl:template name="ShowPrescribeItems">
    <xsl:param name="secPrescribe"/>
    <table class="CDA-ReportItemListTable">
      <tr>
        <th>項次</th>
        <th>處方箋種類註記</th>
        <th>藥品代碼</th>
        <th>藥品商品名稱 <br /> 藥品學名</th>
        <th>劑型</th>
        <th>劑量(單位)</th>
        <th>頻率</th>
        <th>給藥途徑</th>
        <th>給藥日數</th>
        <th>給藥總量(單位)</th>
        <th>實際給藥總量(單位)</th>
        <th>磨粉註記</th>
        <th>註記</th>
		<th>自費註記</th>
      </tr>
      <xsl:for-each select="$secPrescribe/n1:entry">
        <xsl:choose>
          <xsl:when test="n1:substanceAdministration[@negationInd='true']">
            <tr>
              <td colspan="14">當次門診無開立處方用藥紀錄</td>
            </tr>
          </xsl:when>
          <xsl:otherwise>
            <xsl:variable name="supplyRQO" select="n1:substanceAdministration/n1:entryRelationship/n1:supply[@moodCode='RQO']" />
            <xsl:variable name="supplyPRP" select="n1:substanceAdministration/n1:entryRelationship/n1:supply[@moodCode='PRP']" />
            <xsl:variable name="act" select="n1:substanceAdministration/n1:entryRelationship/n1:act[@classCode='ACT']" />
            <tr>
              <td>
                <!--項次-->
                <xsl:value-of select="n1:substanceAdministration/n1:id/@extension" />
              </td>
              <td>
                <!--處方箋種類註記-->
                <xsl:value-of select="$supplyPRP/n1:code/@displayName"/>
              </td>
              <td>
                <!--藥品代碼-->
                <xsl:value-of select="n1:substanceAdministration/n1:code/@code" />
              </td>
              <td>
                <!--藥品商品名稱-->
                <xsl:value-of select="n1:substanceAdministration/n1:consumable/n1:manufacturedProduct/n1:manufacturedLabeledDrug/n1:name" />
                <br />
                <!--藥品學名-->
                <xsl:value-of select="$supplyRQO/n1:product/n1:manufacturedProduct/n1:manufacturedMaterial/n1:name"/>
              </td>
              <td>
                <!--劑型-->
                <xsl:value-of select="n1:substanceAdministration/n1:administrationUnitCode/@displayName" />
              </td>
              <td>
                <!-->劑量(單位)-->
                <xsl:value-of select="n1:substanceAdministration/n1:doseQuantity/@value" />
                (<xsl:value-of select="n1:substanceAdministration/n1:doseQuantity/@unit" />)
              </td>
              <td>
                <!--頻率-->
                <xsl:value-of select="$act/n1:text"/>
              </td>
              <td>
                <!--給藥途徑-->
                <xsl:value-of select="n1:substanceAdministration/n1:routeCode/@displayName" />
              </td>
              <td>
                <!--給藥日數-->
                <xsl:value-of select="n1:substanceAdministration/n1:repeatNumber/@value" />
              </td>
              <td>
                <!--給藥總量(單位)-->
                <xsl:value-of select="$supplyPRP/n1:quantity/@value"/>
                (<xsl:value-of select="$supplyPRP/n1:quantity/@unit"/>)
              </td>
              <td>
                <!--實際給藥總量(單位)-->
                <xsl:value-of select="$supplyRQO/n1:quantity/@value"/>
                (<xsl:value-of select="$supplyRQO/n1:quantity/@unit"/>)
              </td>
              <td>
                <!--磨粉註記-->
                <xsl:value-of select="$supplyRQO/n1:text"/>
              </td>
              <td>
                <!--註記-->
                <xsl:value-of select="n1:substanceAdministration/n1:text" />
              </td>
			  <td>
                <!--自費註記-->
                <xsl:value-of select="n1:substanceAdministration/n1:entryRelationship/n1:act[n1:code[@code='52556-8' and @codeSystem='2.16.840.1.113883.6.1']]/n1:text[node() or string(.)]" />
              </td>
            </tr>
          </xsl:otherwise>
        </xsl:choose>
        
      </xsl:for-each>
    </table>
  </xsl:template>
</xsl:stylesheet>