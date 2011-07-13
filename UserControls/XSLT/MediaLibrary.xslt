<?xml version="1.0"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="html"/>
    <xsl:template match="//topics">
        <table class="MediaSeries">
            <tr class="MediaSeries">
                <xsl:for-each select="topic">
                    <td class="MediaSeries">
                        <img class="MediaSeries bounceshadow">
                        <xsl:attribute name="src">CachedBlob.aspx?guid=<xsl:value-of select="@imageguid" />&amp;width=270</xsl:attribute>
                        </img>
                        <p style="display: none"><xsl:value-of select="@id" /></p>
                    </td>
                <xsl:if test="position() mod 3 = 0">
                    <xsl:text disable-output-escaping="yes">&lt;/tr&gt;&lt;tr&gt;</xsl:text>
                </xsl:if>
                </xsl:for-each>
            </tr>
        </table>
    </xsl:template>
</xsl:stylesheet>

