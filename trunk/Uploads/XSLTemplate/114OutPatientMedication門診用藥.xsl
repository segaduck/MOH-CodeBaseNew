<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:cdp="http://www.hl7.org.tw/EMR/CDocumentPayload/v1.0" xmlns:n1="urn:hl7-org:v3" xmlns:n2="urn:hl7-org:v3/meta/voc" xmlns:n3="http://www.w3.org/1999/xhtml" xmlns:voc="urn:hl7-org:v3/voc" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
<xsl:include href="Common.xsl"/>

<xsl:variable name="version114">
  <xsl:text>114:2011-06-03-00</xsl:text>
</xsl:variable>
  
<xsl:template match="/">
 <top>
   <xsl:apply-templates/>
 </top>
</xsl:template>

<xsl:template match="//n1:ClinicalDocument"  mode="OutPatientMedication">
  <xsl:variable name="title">
    <xsl:choose>
         <xsl:when test="//n1:ClinicalDocument/n1:title">
             <xsl:value-of select="//n1:ClinicalDocument/n1:title"/>
         </xsl:when>
         <xsl:otherwise>門診用藥</xsl:otherwise>
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
        <td colspan="4" class="CDA-ReportDivBar">紀錄資訊</td>
      </tr>
      <tr>
        <th>
          <xsl:text>醫囑編號：</xsl:text>
        </th>
        <td>
          <xsl:value-of select="n1:inFulfillmentOf/n1:order/n1:id/@extension"/>
        </td>
        <!--<th>
          <xsl:text>處方箋種類：</xsl:text>
        </th>
        <td>
          <xsl:call-template name="getCodeAndName">
            <xsl:with-param name="TargetNode" select="n1:inFulfillmentOf/n1:order/n1:code" />
          </xsl:call-template>
        </td>-->
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
      <tr>
        <th>
          <xsl:text>醫師記錄時間：</xsl:text>
        </th>
        <td>
          <xsl:call-template name="formatDate">
            <xsl:with-param name="date" select="n1:author/n1:time/@value" />
          </xsl:call-template>
        </td>
        <th>
          <xsl:text>記錄醫師：</xsl:text>
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
        <td colspan="4" class="CDA-ReportDivBar">用藥紀錄內容</td>
      </tr>
      <xsl:for-each select="n1:component/n1:structuredBody/n1:component/descendant::n1:section">
        <xsl:variable name="LoincCode" select="n1:code/@code" />
        <xsl:choose>
          <!--診斷-->
          <xsl:when test="$LoincCode='29548-5'">
            <xsl:choose>
              <xsl:when test="n1:entry">
                <tr>
                  <th>
                    <xsl:value-of select ="n1:title" />：
                  </th>
                  <td colspan="3">
                    <xsl:for-each select="n1:entry">
                      <xsl:call-template name="getCodeAndName">
                        <xsl:with-param name="TargetNode" select="n1:observation/n1:code" />
                      </xsl:call-template>
                      <br />
                    </xsl:for-each>
                  </td>
                </tr>
              </xsl:when>
              <xsl:otherwise>
                <!--直接顯示既有內容-->
                <tr>
                  <th>
                    <xsl:value-of select ="n1:title" />：
                  </th>
                  <td colspan="3">
                    <xsl:apply-templates select="n1:text"/>
                  </td>
                </tr>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <!--處方-->
          <xsl:when test="$LoincCode='29551-9'">
            <tr>
              <td colspan="4">
                <xsl:choose>
                  <xsl:when test="n1:entry">
                    <table class="CDA-ReportItemListTable">
                      <tr>
                        <th>項次</th>
                        <th>藥品名稱(代碼)</th>
                        <th>藥品商品名稱</th>
                        <th>藥品學名</th>
                        <th>處方箋種類</th>
                        <th>劑型(代碼)</th>
                        <th>劑量(單位)</th>
                        <th>頻率</th>
                        <th>給藥途徑(代碼)</th>
                        <th>給藥日數</th>
                        <th>給藥總量(單位)</th>
                        <th>實際給藥總量(單位)</th>
                        <th>磨粉註記</th>
                        <th>註記</th>
                      </tr>
                      <xsl:for-each select="n1:entry">
                        <tr>
                          <td>
                            <xsl:value-of select="n1:substanceAdministration/n1:id/@extension" />
                          </td>
                          <td >
                            <xsl:call-template name="getCodeAndName">
                              <xsl:with-param name="TargetNode" select="n1:substanceAdministration/n1:code" />
                            </xsl:call-template>
                          </td>
                          <td>
                            <xsl:value-of select="n1:substanceAdministration/n1:consumable/n1:manufacturedProduct/n1:manufacturedLabeledDrug/n1:name" />
                          </td>
                          <td>
                            <xsl:value-of select ="n1:substanceAdministration/n1:entryRelationship/n1:supply[@moodCode='RQO']/n1:product/n1:manufacturedProduct/n1:manufacturedMaterial/n1:name"/>
                          </td>
                          <td>
                            <xsl:value-of select="./n1:substanceAdministration/n1:entryRelationship/n1:supply[@classCode='SPLY' and @moodCode='PRP']/n1:code/@displayName"/>
                          </td>
                          <td>
                            <xsl:call-template name="getCodeAndName">
                              <xsl:with-param name="TargetNode" select="n1:substanceAdministration/n1:administrationUnitCode" />
                            </xsl:call-template>
                          </td>
                          <td>
                            <xsl:value-of select="n1:substanceAdministration/n1:doseQuantity/@value" />
                            (<xsl:value-of select="n1:substanceAdministration/n1:doseQuantity/@unit" />)
                          </td>
                          <td>
                            <xsl:value-of select ="n1:substanceAdministration/n1:entryRelationship/n1:act/n1:text"/>
                          </td>
                          <td>
                            <xsl:call-template name="getCodeAndName">
                              <xsl:with-param name="TargetNode" select="n1:substanceAdministration/n1:routeCode" />
                            </xsl:call-template>
                          </td>
                          <td>
                            <xsl:value-of select ="n1:substanceAdministration/n1:repeatNumber/@value"/>
                          </td>
                          <td>
                            <xsl:value-of select ="n1:substanceAdministration/n1:entryRelationship/n1:supply[@classCode='SPLY' and @moodCode='PRP']/n1:quantity/@value"/>
                            (<xsl:value-of select ="n1:substanceAdministration/n1:entryRelationship/n1:supply[@classCode='SPLY' and @moodCode='PRP']/n1:quantity/@unit"/>)
                          </td>
                          <td>
                            <xsl:value-of select ="n1:substanceAdministration/n1:entryRelationship/n1:supply[@classCode='SPLY' and @moodCode='RQO']/n1:quantity/@value"/>
                            (<xsl:value-of select ="n1:substanceAdministration/n1:entryRelationship/n1:supply[@classCode='SPLY' and @moodCode='RQO']/n1:quantity/@unit"/>)
                          </td>
                          <td>
                            <xsl:value-of select ="n1:substanceAdministration/n1:entryRelationship/n1:supply[@classCode='SPLY' and @moodCode='RQO']/n1:text"/>
                          </td>
                          <td>
                            <xsl:value-of select ="n1:substanceAdministration/n1:text"/>
                          </td>
                        </tr>
                      </xsl:for-each>
                    </table>
                  </xsl:when>
                  <xsl:otherwise>
                    <!--直接顯示預設表格-->
                    <xsl:apply-templates select="n1:text"/>
                  </xsl:otherwise>
                </xsl:choose>
              </td>
            </tr>
          </xsl:when>
        </xsl:choose>
      </xsl:for-each>
    </table>
  </div>
</xsl:template>

</xsl:stylesheet>
