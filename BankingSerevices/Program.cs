using System;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Transactions;

namespace BankingSerevices
{
    [ServiceContract]
    public interface myInterface
    {

        // TODO: Add your service operations here
        // Implementation of service interface.
        [OperationContract]
        string GetFile(string fileName);

        [OperationContract]
        string Storage(string fileName, byte[] fileContents);

        [OperationContract]
        string CreateAccount(string accountNumber, string owner, string balance, string password);

        [OperationContract]
        string Transfer(string source, string destination, string amount);

        [OperationContract]
        string Deposit(string acctNickname, string amount);

        [OperationContract]
        string Withdrawal(string acctNickname, string amount);

        [OperationContract]
        string GetAccountInfo(string owner);

        [OperationContract]
        string GetOwner(string nickname);

        [OperationContract]
        bool AccountExists(string owner);

        [OperationContract]
        bool DeleteAccount(string owner);

        [OperationContract]
        bool UpdatePassword(string owner, string currentPassword, string newPassword1, string newPassword2);
    }

    public class myService : myInterface
    {
        // Returns true if the password in Accounts.xml is updated.
        public bool UpdatePassword(string owner, string currentPassword, string newPassword1, string newPassword2)
        {
            if (newPassword1 != newPassword2)
            {
                return false;
            }
            string XMLLocale = Directory.GetCurrentDirectory() + @"\App_Data\Accounts.xml";
            XDocument xmlDocAccts = new XDocument();
            try
            {
                xmlDocAccts = XDocument.Load(XMLLocale);
                var existingAccount = from c in xmlDocAccts.Root.Elements("Account")
                                      where (c.Element("Owner").Value == owner && c.Element("Password").Value == currentPassword)
                                      select c;
                foreach (var ea in existingAccount)
                {
                    ea.Element("Password").Value = newPassword1;
                    xmlDocAccts.Save(XMLLocale);
                    return true;
                }
            }
            catch { }
            return false;
        }

        // Deletes the account from Accounts.xml
        public bool DeleteAccount(string owner)
        {
            // Check if account exists
            if (!AccountExists(owner))
            {
                return false;
            }
            string XMLLocale = Directory.GetCurrentDirectory() + @"\App_Data\Accounts.xml";
            XDocument xmlDocAccts = new XDocument();
            try
            {
                xmlDocAccts = XDocument.Load(XMLLocale);
                var existingAccount = from c in xmlDocAccts.Root.Elements("Account")
                                      where (c.Element("Owner").Value == owner)
                                      select c;
                foreach (var ea in existingAccount)
                {
                    ea.Remove();
                    xmlDocAccts.Save(XMLLocale);
                    return true;
                }
            }
            catch { }
            return false;
        }

        // Adds amount to the balance of the account & returns the new balance. 
        // Invokes the RESTful Addition service. 
        public string Deposit(string acctNickname, string amount)
        {
            if (acctNickname == "" || amount == "")
            {
                return "Invalid input";
            }
            try
            {
                Convert.ToDouble(amount);
            }
            catch
            {
                return "Invalid input";
            }
            string pathToFile = Directory.GetCurrentDirectory() + @"\App_Data\Accounts.xml";
            XDocument xmlDocAccts = new XDocument();
            string returnBalance = "Invalid";

            try
            {
                xmlDocAccts = XDocument.Load(pathToFile);
                var account = from c in xmlDocAccts.Root.Elements("Account")
                              where (c.Element("Nickname").Value == acctNickname)
                              select c;
                foreach (var d in account)
                {
                    // Call to RESTful Addition service
                    returnBalance = additionRest(d.Element("Balance").Value, amount);
                    try
                    {
                        Convert.ToDouble(returnBalance);
                        d.Element("Balance").Value = returnBalance;
                    }
                    catch
                    {
                        return returnBalance;
                    }
                }

                xmlDocAccts.Save(pathToFile);
                return returnBalance;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // Given the account nickname & the amount, it withdrawals the amount & returns the new balance.
        // Invokes the RESTful Subtraction service.
        public string Withdrawal(string acctNickname, string amount)
        {
            if (acctNickname == "" || amount == "")
            {
                return "Invalid input";
            }
            try
            {
                Convert.ToDouble(amount);
            }
            catch
            {
                return "Invalid input";
            }
            string pathToFile = Directory.GetCurrentDirectory() + @"\App_Data\Accounts.xml";

            XDocument xmlDocAccts = new XDocument();
            string returnBalance = "Invalid";

            try
            {
                xmlDocAccts = XDocument.Load(pathToFile);
                var account = from c in xmlDocAccts.Root.Elements("Account")
                              where (c.Element("Nickname").Value == acctNickname)
                              select c;
                foreach (var d in account)
                {
                    if (d.Element("Balance").Value == "") // Checks if element has a value.
                    {
                        d.Element("Balance").Value = "0";

                    }
                    // Call to RESTful Addition service
                    returnBalance = subtractionRest(d.Element("Balance").Value, amount);
                    if (Convert.ToDouble(returnBalance) < 0)
                    {
                        return "Insufficient funds";
                    }
                    d.Element("Balance").Value = returnBalance;
                }

                xmlDocAccts.Save(pathToFile);
                return returnBalance;
            }
            catch
            {
                return "Invalid";
            }
        }

        // Given the source & destination account nicknames & the amount, it calls the RESTful Subtraction 
        // service to withdrawal the amound from the source account & calls the RESTful Addition service to 
        // deposit the amount into the destination account. The new balance of the source account is returned.
        // Note: if the amount type is invald, the amount will be assigned 0.
        public string Transfer(string source, string destination, string amount)
        {
            if (source == "" || destination == "" || amount == "")
            {
                return "Invalid input";
            }
            try
            {
                Convert.ToDouble(amount);
            }
            catch
            {
                return "Invalid amount";
            }
            string returnBalance = "Invalid";
            string XMLLocale = Directory.GetCurrentDirectory() + @"\App_Data\Accounts.xml";
            XDocument xmlDocAccts = new XDocument();

            try
            {
                xmlDocAccts = XDocument.Load(XMLLocale);
                var transferSource = from c in xmlDocAccts.Root.Elements("Account")
                                     where (c.Element("Nickname").Value == source)
                                     select c;

                var transferDestination = from d in xmlDocAccts.Root.Elements("Account")
                                          where (d.Element("Nickname").Value == destination)
                                          select d;
                foreach (var ts in transferSource)
                {
                    foreach (var td in transferDestination)
                    {
                        returnBalance = subtractionRest(ts.Element("Balance").Value, amount);
                        if (Convert.ToDouble(returnBalance) < 0)
                        {
                            return "Insufficient funds";
                        }
                        ts.Element("Balance").Value = returnBalance;
                        td.Element("Balance").Value = additionRest(td.Element("Balance").Value, amount);
                    }
                }
                xmlDocAccts.Save(XMLLocale);
                return returnBalance;
            }
            catch
            {
                return "Transaction Fault";
            }
        }

        // Invokes the addition RESTful service to add the amount to the balance.
        // Returns the new balance.
        private string additionRest(string balance, string amount)
        {
            try
            {
                // Create the base address
                Uri baseUri = new Uri("http://localhost:54118/Service.svc");
                // Create the from tree root to the child node
                UriTemplate myTemplate = new UriTemplate("add2/{operand1}/{operand2}");
                // Assign values to variables to complete Uri
                Uri completeUri = myTemplate.BindByPosition(baseUri, balance, amount);
                WebClient proxy = new WebClient();
                byte[] abc = proxy.DownloadData(completeUri);
                Stream strm = new MemoryStream(abc);
                DataContractSerializer obj = new DataContractSerializer(typeof(string));
                return obj.ReadObject(strm).ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // Invokes the subtraction RESTful service to subtract the amount from the balance.
        // Returns the new balance.
        private string subtractionRest(string balance, string amount)
        {
            try
            {
                // Create the base address
                Uri baseUri = new Uri("http://localhost:54118/Service.svc");
                // Create the from tree root to the child node
                UriTemplate myTemplate = new UriTemplate("sub2/{operand1}/{operand2}");
                // Assign values to variables to complete Uri
                Uri completeUri = myTemplate.BindByPosition(baseUri, balance, amount);
                WebClient proxy = new WebClient();
                byte[] abc = proxy.DownloadData(completeUri);
                Stream strm = new MemoryStream(abc);
                DataContractSerializer obj = new DataContractSerializer(typeof(string));
                return obj.ReadObject(strm).ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // Given the file name, it returns the contents of the file as a string. 
        public string GetFile(string fileName)
        {
            try
            {
                string path = Directory.GetCurrentDirectory() + @"\Files";

                string serverPathAndFilename = Path.Combine(path, fileName);
                if (File.Exists(serverPathAndFilename))
                {
                    string fileContents = File.ReadAllText(serverPathAndFilename, Encoding.UTF8);
                    return fileContents;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "File Not found";
        }
        // GIven a file name & its contents, it stores the file in the Files directory. 
        // It returns the URL of the file on the server. 
        public string Storage(string fileName, byte[] fileContents)
        {
            try
            {
                string path = Directory.GetCurrentDirectory() + @"\Files";
                string serverPathAndFilename = Path.Combine(path, fileName);

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                int count = 1;
                int index;
                string suffix = "";
                while (File.Exists(serverPathAndFilename))
                {
                    // Add a number to the file name if it exists
                    if (fileName.Contains("(" + (count - 1).ToString() + ")"))
                    {
                        fileName = fileName.Replace("(" + (count - 1).ToString() + ")", "(" + count.ToString() + ")");
                    }
                    else
                    {
                        index = fileName.LastIndexOf(".");
                        suffix = "(" + count.ToString() + ")";
                        fileName = fileName.Insert(index, suffix);
                    }
                    serverPathAndFilename = Path.Combine(path, fileName);
                    count++;
                }
                File.WriteAllBytes(serverPathAndFilename, fileContents);
                return serverPathAndFilename;
            }
            catch
            {
                return "Storage Failed";
            }
        }

        // Given the account owner, it returns the account number, nickname, & balance. 
        public string GetAccountInfo(string owner)
        {
            string acctInfo = "";
            string XMLLocale = Directory.GetCurrentDirectory() + @"\App_Data\Accounts.xml";

            XDocument xmlDocAccts = new XDocument();
            try
            {
                xmlDocAccts = XDocument.Load(XMLLocale);
                var infoElement = from c in xmlDocAccts.Root.Elements("Account")
                                  where (c.Element("Owner").Value == owner)
                                  select c;

                foreach (var ie in infoElement)
                {
                    acctInfo = ie.Element("AccountNumber").Value;
                    acctInfo = acctInfo + " " + ie.Element("Nickname").Value;
                    acctInfo = acctInfo + " " + ie.Element("Balance").Value;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return acctInfo;
        }

        // Given the account nickname, it returns the account owner. 
        public string GetOwner(string nickname)
        {
            string owner = "Invalid entry";
            string XMLLocale = Directory.GetCurrentDirectory() + @"\App_Data\Accounts.xml";

            XDocument xmlDocAccts = new XDocument();
            try
            {
                xmlDocAccts = XDocument.Load(XMLLocale);
                var infoElement = from c in xmlDocAccts.Root.Elements("Account")
                                  where (c.Element("Nickname").Value == nickname)
                                  select c;

                foreach (var ie in infoElement)
                {
                    owner = ie.Element("Owner").Value;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return owner;
        }

        // Returns true if the owner has an account in Accounts.xml.
        public bool AccountExists(string owner)
        {
            string XMLLocale = Directory.GetCurrentDirectory() + @"\App_Data\Accounts.xml";

            XDocument xmlDocAccts = new XDocument();
            try
            {
                xmlDocAccts = XDocument.Load(XMLLocale);
                var infoElement = from c in xmlDocAccts.Root.Elements("Account")
                                  where (c.Element("Owner").Value == owner)
                                  select c;

                foreach (var ie in infoElement)
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        // Creates an account by entering an account number, owner & the balance. 
        // It generates a nickname for the account via calling GenerateNickname
        // & generates a password via calling HashService RESTful service. 
        // The account number, nickname, password, & balance are stored in Accounts.xml. 
        // The account nickname is returned. Note: invalid balance types will be assigned 0.
        public string CreateAccount(string accountNumber, string owner, string balance, string password)
        {
            // Tests if balance is in valid format.
            try
            {
                double testBalance = Convert.ToDouble(balance);
            }
            catch
            {
                balance = "0";
            }
            string nickname = "Invalid Nickname";
            try
            {
                nickname = GenerateNickname(accountNumber);
            }
            catch
            {
                return nickname;
            }
            // Check if account already exists
            if (AccountExists(owner))
            {
                return "Account Exists";
            }
            string XMLLocale = Directory.GetCurrentDirectory() + @"\App_Data\Accounts.xml";
            XDocument xmlDocAccts = new XDocument();
            try
            {
                xmlDocAccts = XDocument.Load(XMLLocale);
                XElement xmlElement = new XElement("Account",
                    new XElement("AccountNumber", accountNumber),
                    new XElement("Nickname", nickname),
                    new XElement("Owner", owner),
                    new XElement("Password", password),
                    new XElement("Balance", balance));
                xmlDocAccts.Element("Accounts").Add(xmlElement);
                xmlDocAccts.Save(XMLLocale);
            }
            catch (Exception ex)
            {
                string exMsg = ex.Message.ToLower().ToString();
                if (exMsg.Contains("root") && exMsg.Contains("element") && (exMsg.Contains("not found") || exMsg.Contains("missing")))
                {
                    xmlDocAccts = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"),
                    new XComment("Accounts Database"),
                    new XElement("Accounts",
                    new XElement("Account",
                    new XElement("AccountNumber", accountNumber),
                    new XElement("Nickname", nickname),
                    new XElement("Owner", owner),
                    new XElement("Password", password),
                    new XElement("Balance", balance))));
                    xmlDocAccts.Save(XMLLocale);
                }
            }
            return nickname;
        }

        // Converts numbers to memorable phrases.
        // Best to use numbers with the number of digits divisible by four.  
        // Note: any non-number inputs will not be assigned a word. 
        private string GenerateNickname(string number)
        {
            try
            {
                int num = Convert.ToInt32(number);
            }
            catch
            {
                return "Invalid Entry";
            }
            string words = "";
            int iteration = 0;
            foreach (char c in number)
            {
                int mod = iteration % 4;
                switch (mod)
                {
                    case 0:
                        switch (c)
                        {
                            case '1':
                                words += "Big";
                                break;
                            case '2':
                                words += "Small";
                                break;
                            case '3':
                                words += "Beautiful";
                                break;
                            case '4':
                                words += "Resilient";
                                break;
                            case '5':
                                words += "Skinny";
                                break;
                            case '6':
                                words += "Pretty";
                                break;
                            case '7':
                                words += "Playful";
                                break;
                            case '8':
                                words += "Handsome";
                                break;
                            case '9':
                                words += "Smart";
                                break;
                            case '0':
                                words += "Humble";
                                break;
                        }
                        break;
                    case 1:
                        switch (c)
                        {
                            case '1':
                                words += "Bruin";
                                break;
                            case '2':
                                words += "SunDevil";
                                break;
                            case '3':
                                words += "Cat";
                                break;
                            case '4':
                                words += "Dog";
                                break;
                            case '5':
                                words += "Alien";
                                break;
                            case '6':
                                words += "Turtle";
                                break;
                            case '7':
                                words += "Dolphin";
                                break;
                            case '8':
                                words += "Sloth";
                                break;
                            case '9':
                                words += "Eagle";
                                break;
                            case '0':
                                words += "Shark";
                                break;
                        }
                        break;
                    case 2:
                        switch (c)
                        {
                            case '1':
                                words += "Plays";
                                break;
                            case '2':
                                words += "Eats";
                                break;
                            case '3':
                                words += "Sleeps";
                                break;
                            case '4':
                                words += "Runs";
                                break;
                            case '5':
                                words += "Dances";
                                break;
                            case '6':
                                words += "Jumps";
                                break;
                            case '7':
                                words += "Reads";
                                break;
                            case '8':
                                words += "Writes";
                                break;
                            case '9':
                                words += "Speaks";
                                break;
                            case '0':
                                words += "Studies";
                                break;
                        }
                        break;
                    case 3:
                        switch (c)
                        {
                            case '1':
                                words += "Loudly";
                                break;
                            case '2':
                                words += "Quickly";
                                break;
                            case '3':
                                words += "Slowly";
                                break;
                            case '4':
                                words += "Quietly";
                                break;
                            case '5':
                                words += "Happily";
                                break;
                            case '6':
                                words += "Easily";
                                break;
                            case '7':
                                words += "Smoothly";
                                break;
                            case '8':
                                words += "Chamingly";
                                break;
                            case '9':
                                words += "Joyfully";
                                break;
                            case '0':
                                words += "Repeatedly";
                                break;
                        }
                        break;
                }
                iteration++;
            }
            return words;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            // Create a URI instance to myService as the base address.
            Uri baseAddress = new Uri("http://localhost:8000/Service");
            // A new service host instance to host the service.
            ServiceHost selfHost = new ServiceHost(typeof(myService), baseAddress);
            try
            {
                // Add a service endpoint with contract & binding'
                selfHost.AddServiceEndpoint(typeof(myInterface), new WSHttpBinding(), "myService");
                // Add meta data for platform-independent access. 
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                // Enable the metadata
                smb.HttpGetEnabled = true;
                // Add here
                selfHost.Description.Behaviors.Add(smb);
                // Ensure IncludeExceptionDtailinFaults setting is on
                ServiceDebugBehavior debug = selfHost.Description.Behaviors.Find<ServiceDebugBehavior>();
                if (!debug.IncludeExceptionDetailInFaults)
                {
                    debug.IncludeExceptionDetailInFaults = true;
                }


                // Start service & wait for request.
                selfHost.Open();
                Console.WriteLine("myService is ready to take requests");
                Console.WriteLine("Press enter to quit service.");
                Console.ReadLine();
                // Close the service host base to shut down the service.
                selfHost.Close();
            }
            catch (CommunicationException ce)
            {
                Console.WriteLine("An exception occurred: {0}", ce.Message);
                selfHost.Abort();
            }
        }
    }
}
