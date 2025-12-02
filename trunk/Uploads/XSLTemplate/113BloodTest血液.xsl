<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:cdp="http://www.hl7.org.tw/EMR/CDocumentPayload/v1.0" xmlns:n1="urn:hl7-org:v3" xmlns:n2="urn:hl7-org:v3/meta/voc" xmlns:n3="http://www.w3.org/1999/xhtml" xmlns:voc="urn:hl7-org:v3/voc" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
<xsl:include href="Common.xsl"/>

<xsl:variable name="version113">
  <xsl:text>113:2011-06-03-00</xsl:text>
</xsl:variable>

<xsl:template match="/">
 <top>
   <xsl:apply-templates/>
 </top>
</xsl:template>

<xsl:template match="//n1:ClinicalDocument" mode="BloodTest">
  <xsl:variable name="title">
    <xsl:choose>
         <xsl:when test="//n1:ClinicalDocument/n1:title">
             <xsl:value-of select="//n1:ClinicalDocument/n1:title"/>
         </xsl:when>
         <xsl:otherwise>檢驗報告</xsl:otherwise>
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
        <td colspan="4" class="CDA-ReportDivBar">檢驗資訊</td>
      </tr>
      <tr>
        <th>
          <xsl:text>檢驗單號：</xsl:text>
        </th>
        <td>
          <xsl:value-of select="n1:inFulfillmentOf/n1:order/n1:id/@extension"/>
        </td>
      </tr>
      <tr>
        <th>
          <xsl:text>採檢日期時間：</xsl:text>
        </th>
        <td>
          <xsl:call-template name="formatDate">
            <xsl:with-param name="date" select="n1:componentOf/n1:encompassingEncounter/n1:effectiveTime/@value" />
          </xsl:call-template>
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
      <tr>
        <td colspan="4" class="CDA-ReportDivBar">報告內容</td>
      </tr>
      <!--Report Content-->
      <xsl:for-each select="n1:component/n1:structuredBody/n1:component/descendant::n1:section">
        <xsl:variable name="LoincCode" select="n1:code/@code" />
        <xsl:choose>
          <xsl:when test="$LoincCode='30954-2'">
            <xsl:choose>
              <xsl:when test="n1:entry">
                <xsl:for-each select="n1:entry">                
                <tr>
                  <th>
                    <xsl:text>檢驗名稱(代碼)：</xsl:text>
                  </th>
                  <td colspan="3">
                    <xsl:call-template name="getCodeAndName">
                      <xsl:with-param name="TargetNode" select="n1:organizer/n1:code/n1:translation" />
                    </xsl:call-template>
                  </td>
                </tr>
                <tr>
                  <th>
                    <xsl:text>收件日期時間：</xsl:text>
                  </th>
                  <td colspan="3">
                    <xsl:call-template name="formatDate">
                      <xsl:with-param name="date" select="n1:organizer/n1:effectiveTime/@value" />
                    </xsl:call-template>
                  </td>
                </tr>
                <tr>
                  <th>
                    <xsl:text>檢體類別：</xsl:text>
                  </th>
                  <td colspan="3">
                    <xsl:call-template name="getCodeAndName">
                      <xsl:with-param name="TargetNode" select="n1:organizer/n1:specimen/n1:specimenRole/n1:specimenPlayingEntity/n1:code" />
                    </xsl:call-template>
                    (<xsl:value-of select="n1:organizer/n1:specimen/n1:specimenRole/n1:specimenPlayingEntity/n1:name" />)
                  </td>
                </tr>
                <tr>
                  <th>
                    <xsl:text>檢體來源：</xsl:text>
                  </th>
                  <td colspan="3">
                    <xsl:value-of select="n1:organizer/n1:specimen/n1:specimenRole/n1:specimenPlayingEntity/n1:desc" />
                  </td>
                </tr>
                <tr>
                  <td colspan="4">
                    <table class="CDA-ReportItemListTable">
                      <tr>
                        <th>項次</th>
                        <th>日期時間</th>
                        <th>項目名稱</th>
                        <th>結果</th>
                        <th>參考值</th>
                        <th>檢驗方法</th>
                        <th>備註</th>
                      </tr>
                      <xsl:for-each select="n1:organizer/n1:component">
                        <tr>
                          <td>
                            <xsl:value-of select="n1:observation/n1:id/@extension" />
                          </td>
                          <td>
                            <xsl:call-template name="formatDate">
                              <xsl:with-param name="date" select="n1:observation/n1:effectiveTime/@value" />
                            </xsl:call-template>
                          </td>
                          <td>
                            <xsl:value-of select="n1:observation/n1:code/@displayName" />
                          </td>
                          <td>
                            <xsl:choose>
                              <xsl:when test="n1:observation/n1:value/@xsi:type='ST'">
                                <xsl:value-of select="n1:observation/n1:value" />
                              </xsl:when>
                              <xsl:when test="n1:observation/n1:value/@xsi:type='PQ'">
                                <xsl:value-of select="n1:observation/n1:value/@value" />
                                (<xsl:value-of select="n1:observation/n1:value/@unit" />)
                              </xsl:when>
                              <xsl:when test="n1:observation/n1:value/@xsi:type='IVL_PQ'">
                                <xsl:value-of select="n1:observation/n1:value/n1:low/@value" />
                                (<xsl:value-of select="n1:observation/n1:value/n1:low/@unit" />)
                                ~
                                <xsl:value-of select="n1:observation/n1:value/n1:high/@value" />
                                (<xsl:value-of select="n1:observation/n1:value/n1:high/@unit" />)
                              </xsl:when>
                            </xsl:choose>
                          </td>
                          <td>
                            <xsl:choose>
                              <xsl:when test="n1:observation/n1:referenceRange/n1:observationRange/n1:value/@xsi:type='ST'">
                                <xsl:value-of select="n1:observation/n1:referenceRange/n1:observationRange/n1:value" />
                              </xsl:when>
                              <xsl:when test="n1:observation/n1:referenceRange/n1:observationRange/n1:value/@xsi:type='PQ'">
                                <xsl:value-of select="n1:observation/n1:referenceRange/n1:observationRange/n1:value/@value" />
                                (<xsl:value-of select="n1:observation/n1:referenceRange/n1:observationRange/n1:value/@unit" />)
                              </xsl:when>
                              <xsl:when test="n1:observation/n1:referenceRange/n1:observationRange/n1:value/@xsi:type='IVL_PQ'">
                                <xsl:value-of select="n1:observation/n1:referenceRange/n1:observationRange/n1:value/n1:low/@value" />
                                (<xsl:value-of select="n1:observation/n1:referenceRange/n1:observationRange/n1:value/n1:low/@unit" />)
                                ~
                                <xsl:value-of select="n1:observation/n1:referenceRange/n1:observationRange/n1:value/n1:high/@value" />
                                (<xsl:value-of select="n1:observation/n1:referenceRange/n1:observationRange/n1:value/n1:high/@unit" />)
                              </xsl:when>
                            </xsl:choose>
                          </td>
                          <td>
                            <xsl:value-of select="n1:observation/n1:methodCode/@displayName"/>
                          </td>
                          <td>
                            <xsl:value-of select="n1:observation/n1:text"/>
                          </td>
                        </tr>
                      </xsl:for-each>
                    </table>
                  </td>
                </tr>
                </xsl:for-each>
              </xsl:when>
              <xsl:otherwise>
                <!-- 直接顯示預設表格 -->
                <tr>
                  <td colspan="4">
                    <xsl:apply-templates select="n1:text"/>
                  </td>
                </tr>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
        </xsl:choose>
      </xsl:for-each>
    </table>
  </div>
</xsl:template>

</xsl:stylesheet>
