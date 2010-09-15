REM Use "tastyserver" as the test password for the server certificate.

"C:\Program Files\Microsoft SDKs\Windows\v6.0A\bin\makecert" -r -pe -n "CN=Tasty Server" -sky exchange TastyServer.cer -sv TastyServer.pvk
"C:\Program Files\Microsoft SDKs\Windows\v6.0A\bin\pvk2pfx" -pvk TastyServer.pvk -pi tastyserver -spc TastyServer.cer -pfx TastyServer.pfx -po tastyserver
rm TastyServer.pvk
rm TastyServer.cer

REM Use "tastyclient" as the test password for the client certificate.

"C:\Program Files\Microsoft SDKs\Windows\v6.0A\bin\makecert" -r -pe -n "CN=Tasty Client" -sky exchange TastyClient.cer -sv TastyClient.pvk
"C:\Program Files\Microsoft SDKs\Windows\v6.0A\bin\pvk2pfx" -pvk TastyClient.pvk -pi tastyclient -spc TastyClient.cer -pfx TastyClient.pfx -po tastyclient
rm TastyClient.pvk