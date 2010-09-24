<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:tasty="http://tastycodes.com/tasty-dll/mailmodel/"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl tasty">
  <xsl:output method="html"/>
  <!-- This is an example email template, assuming a MailModel with the following properties: -->
  <!-- RecipientName, WebsiteName, WebsiteUrl -->
  <xsl:template match="/">
    <head>
      <title>Welcome to 
        <xsl:value-of select="//tasty:WebsiteName"/>
      </title>
    </head>
    <body>
      <table>
        <tr>
          <td>
            <p>
              Hey <strong>
                <xsl:value-of select="//tasty:RecipientName"/>
              </strong>,
            </p>

            <p>
              Welcome to
              <a>
                <xsl:attribute name="href">
                  <xsl:value-of select="//tasty:WebsiteUrl"/>
                </xsl:attribute>
                <xsl:value-of select="//tasty:WebsiteName"/>
              </a> - boy
              are we glad to have you.
            </p>

            <p>
              Stay classy.
            </p>
          </td>
        </tr>
      </table>
    </body>
  </xsl:template>
</xsl:stylesheet>