# BankingServices

This was created using Visual Studio 2017.

To try these services...

1. Download the BankingRestServices repository
2. Double click the BankingRestServices.sln file
3. Under the BankingRestServices project, right click the Service.svc file & select 'View in Browser'
4. Under the HashService project, right click the Service.svc file & select 'View in Browser'

5. Download the BankingServices repository
6. Open visual studio as an administrator
7. Select the 'File' tab, select 'Open', select 'Project/Solution...', & then select the BankingServices.sln file
8. Select the 'Debug' tab & then select 'Start Without Debugging'

9. Download the BankingServicesTryMe repository
10. Double click the BankingServicesTryMe.sln file
11. Right click the Default.aspx file & select 'Set as Start Page'
12. Select the Debug tab & then select 'Start Without Debugging'


These SOAP services are consumed by the SOCBankingWebApp repository. They are dependent on the BankingRestServices repository, i.e. it consumes those RESTful services. This is meant to be tested with the BankingServicesTryMe repository. This project is self-hosted, so visual studio must be started as an administrator.