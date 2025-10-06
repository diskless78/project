 public static async Task<bool> LoginForTestOnly(string userName, string userPassword, string telegramNickName)
 {
     string chromePath = FuckTheLifeToFindTheLuck.FindChromePath();
     if (string.IsNullOrEmpty(chromePath))
     {
         FuckTheLifeToFindTheLuck.LogMessage("Chrome is not installed. Please download and install Google Chrome", AppConst.findtheluckLogFile);
         return false;
     }

     var chromeDriverService = ChromeDriverService.CreateDefaultService();
     chromeDriverService.HideCommandPromptWindow = true;
     var options = new ChromeOptions();
     options.BinaryLocation = chromePath;
     //options.AddArgument("--disable-infobars");
     //options.AddExcludedArgument("enable-automation");
     //options.AddUserProfilePreference("credentials_enable_service", false);
     //options.AddUserProfilePreference("profile.password_manager_enabled", false);
     //options.AddArgument("--headless");
     //options.AddArgument("--disable-gpu");
     options.AddUserProfilePreference("profile.default_content_settings.geolocation", 1);

     using (var driver = new ChromeDriver(chromeDriverService, options))
     {
         try
         {
             driver.Navigate().GoToUrl("https://passport.central.co.th/adfs/ls/IdpInitiatedSignOn.aspx?loginToRp=https://central.wta-eu3.wfs.cloud/workforce/Mobile.do");

             WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(120));
             IWebElement usernameInput = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("userNameInput")));
             IWebElement passwordInput = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("passwordInput")));
             IWebElement submitButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("submitButton")));

             usernameInput.SendKeys(userName);
             passwordInput.SendKeys(userPassword);
             submitButton.Click();

             wait.Until(d => d.Url.Contains("jsessionid"));
             await Task.Delay(10000);

             try
             {
                 
                 // Go to the My Employee
                 IWebElement employeeSpan = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//span[contains(@class, 'x-button-label') and contains(text(), 'Self-Service')]")));
                 string employeeSpanId = employeeSpan.GetAttribute("id");
                 if (!string.IsNullOrEmpty(employeeSpanId))
                 {
                     driver.FindElement(By.Id(employeeSpanId)).Click();
                     await Task.Delay(5000);
                 }
                 else
                 {
                     FuckTheLifeToFindTheLuck.LogMessage("No ID found for Self-Service button", AppConst.findtheluckLogFile);
                 }
                 // Go to My Timesheet
                 IWebElement timesheetDiv = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[@class='menuList-itemLabel menuList-itemLabel-withIcon' and contains(text(), 'My Timesheet')]")));
                 timesheetDiv.Click();
                 await Task.Delay(5000);


                 if (driver is IJavaScriptExecutor js)
                 {
                     js.ExecuteScript("document.body.style.zoom='50%'");
                 }
                 else
                 {
                     FuckTheLifeToFindTheLuck.LogMessage("Error while zoom the browser", AppConst.findtheluckLogFile);
                 }

                 await FuckTheLifeToFindTheLuck.CaptureScreenshot(driver, userName, "Test", "Sucessful for ", telegramNickName);

                 //Find Log Out button
                 IWebElement logOutSpan = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//span[contains(@class, 'x-button-label') and contains(text(), 'Log Out')]")));
                 string logOutSpanId = logOutSpan.GetAttribute("id");
                 if (!string.IsNullOrEmpty(logOutSpanId))
                 {
                     driver.FindElement(By.Id(logOutSpanId)).Click();
                 }
                 else
                 {
                     FuckTheLifeToFindTheLuck.LogMessage("Log Out Button Element_ID Missing", AppConst.findtheluckLogFile);
                 }

                 return true;
             }
             catch (NoSuchElementException)
             {
                 FuckTheLifeToFindTheLuck.LogMessage("Buttons Not Found", AppConst.findtheluckLogFile);
                 return false;
             }
         }
         catch (Exception ex)
         {

             FuckTheLifeToFindTheLuck.LogMessage("Unsuccessful Login: " + ex.ToString(), AppConst.findtheluckLogFile);
             await FuckTheLifeToFindTheLuck.CaptureScreenshot(driver, userName, "ErrorCheckIn", "Unsucessful Login for ", telegramNickName);
             return false;
         }
         finally
         {
             driver.Quit();
         }
     }
 }