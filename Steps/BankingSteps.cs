using BankSystem.Tests.Model;
using BankSystem.Tests.Utilities;
using NUnit.Framework;
using RestAssured.Request;
using RestAssured.Response;
using RestAssured.Response.Logging;
using SpecFlow.Internal.Json;
using RestAssured.Response.Deserialization;
using static RestAssured.Dsl;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Net.Mime;
using System;
using Namotion.Reflection;

namespace BankSystem.Tests.Steps
{
    [Binding]
    public class BankingSteps
    {
        private static string outputFolder;
        String baseurl = "http://localhost:5000";
        String endpoint = "";
        String contentType = "application/json";
        String method = "";
        Object payload = null;
        VerifiableResponse? response;
        BankingModels? cuR;
        UserList users;
        UserId userId;
        User? user;
        Account account;
        List<Account> accounts;
        CreateAccountResponse createAccountResponse;
        Message msg;

        [Given(@"baseurl is ""([^""]*)""")]
        public void GivenBaseurlIs(string p0)
        {
            baseurl = p0;
        }

        [Given(@"the endpoint is ""([^""]*)""")]
        public void GivenTheEndpointIs(string p0)
        {
            endpoint = p0;
        }

        [Given(@"the payload is random username")]
        public void GivenThePayloadIsRandomUsername()
        {
            String name = Utils.GenerateRandomString(10);
            payload = new CreateUserRequest
            {
                Name = name
            };
        }

        [Given(@"content type is ""([^""]*)""")]
        public void GivenContentTypeIs(string p0)
        {
            contentType = p0;
        }

        [Given(@"method is ""([^""]*)""")]
        public void GivenMethodIs(string method)
        {
            this.method = method;
        }

        [When(@"the request is made")]
        public void WhenTheRequestIsMade()
        {
            ExecutableRequest er = Given().ContentType(contentType);//.Log(RequestLogLevel.All);
            if (payload != null) {
                er.Body(payload);
            }
            if (method.ToLower().Trim().Equals("get")) 
            {
                response = er.Get(baseurl + endpoint).Log(ResponseLogLevel.All);
            } 
            else if (method.ToLower().Trim().Equals("post")) 
            {
                response = er.Post(baseurl + endpoint).Log(ResponseLogLevel.All);
            }
            else if (method.ToLower().Trim().Equals("delete"))
            {
                response = er.Delete(baseurl + endpoint).Log(ResponseLogLevel.All);
            }
            else if (method.ToLower().Trim().Equals("patch"))
            {
                response = er.Patch(baseurl + endpoint).Log(ResponseLogLevel.All);
            }
        }

        [Then(@"the status code is (\d+)$")]
        public void ThenTheStatusCodeIs(int p0)
        {
            response.Then().StatusCode(p0);
        }

        [Then(@"the response message for the created user should contain ""([^""]*)""")]
        public void ThenTheResponseMessageShouldContain(string p0)
        {
            cuR = (BankingModels)response.DeserializeTo(typeof(BankingModels), DeserializeAs.Json);
            Assert.That(cuR.Message.Contains(p0));
        }

        [Then(@"check the new user is created with an account of minimum balance of (.*)")]
        public void ThenCheckAnAccountIsCreatedWithMinimumBalanceOf(int p0)
        {
            response = Given().ContentType(contentType).Get(baseurl + "/api/users/" + cuR?.UserId);
            user = (User)response.DeserializeTo(typeof(User), DeserializeAs.Json);
            Assert.That(Regex.IsMatch(user.userId, "[^0-9]"));
            Assert.That(user.Accounts.Count == 1);
            Assert.That(((int)user.Accounts.First().balance == p0));
        }

        [Given(@"I create a new user")]
        public void GivenICreateARandomUser()
        {
            GivenThePayloadIsRandomUsername();
            response = Given().ContentType(contentType).Body(payload).Post(baseurl + "/api/users").Log(ResponseLogLevel.All);
            cuR = (BankingModels)response.DeserializeTo(typeof(BankingModels), DeserializeAs.Json);
            userId = new UserId { Id = cuR.UserId, Name = ((CreateUserRequest)payload).Name };
        }

        [Given(@"I create a new user with name ""([^""]*)""")]
        public void GivenICreateARandomUser(string name)
        {
            payload = new CreateUserRequest
            {
                Name = name
            };
            response = Given().ContentType(contentType).Body(payload).Post(baseurl + "/api/users");
            cuR = (BankingModels)response.DeserializeTo(typeof(BankingModels), DeserializeAs.Json);
            msg = new Message { msg = cuR.Message };
            userId = new UserId { Id = cuR.UserId, Name = ((CreateUserRequest)payload).Name };
        }

        [Given(@"I create (\+d) new users")]
        public void GivenICreateSomeUsers(int num)
        {
            for(int i = 0; i < num; i++) { 
                GivenThePayloadIsRandomUsername();
                response = Given().ContentType(contentType).Body(payload).Post(baseurl + "/api/users");
                cuR = (BankingModels)response.DeserializeTo(typeof(BankingModels), DeserializeAs.Json);
                userId = new UserId { Id = cuR.UserId , Name = ((CreateUserRequest)payload).Name };
            }
        }

        [Given(@"I have a random user with accounts")]
        public void GivenIHaveARandomUserWithAccount()
        {
            response = Given().ContentType(contentType).Get(baseurl + "/api/users");
            List<UserId> userL = (List<UserId>)response.DeserializeTo(typeof(List<UserId>), DeserializeAs.Json);
            foreach (UserId userID in userL) 
            {
                userId = userID;
                response = Given().ContentType(contentType).Get(baseurl + "/api/users/" + userID.Id + "/accounts").Log(ResponseLogLevel.All);
                accounts = (List<Account>)response.DeserializeTo(typeof(List<Account>), DeserializeAs.Json);
                if (accounts.Count > 0) break;
            }
            if (accounts.Count == 0) {
                Assert.Fail("No user with accounts associated. Please add some.");
            }
            Console.WriteLine("Selected user Id: " + userId.Id);
            Console.WriteLine("Count of accounts associated: " + accounts.Count);
        }

        [Given(@"I pick a random Account for the user")]
        public void GivenIPickARandomAccountForTheUser()
        {
            response = Given().ContentType(contentType).Get(baseurl + "/api/users/" + userId.Id + "/accounts").Log(ResponseLogLevel.All);
            accounts = (List<Account>)response.DeserializeTo(typeof(List<Account>), DeserializeAs.Json);
            Random random = new Random();
            int index = random.Next(0, accounts.Count);
            account = accounts[index];
            Console.WriteLine("Selected Account Id: " + account.accountId);
            Console.WriteLine("Selected Account balance: " + account.balance);
        }


        [When(@"I added an account for the user")]
        public void WhenIAddedAnAccountForTheUser()
        {
            response = Given().ContentType(contentType).Post(baseurl + "/api/users/" + userId.Id + "/accounts").Log(ResponseLogLevel.All);
            createAccountResponse = (CreateAccountResponse)response.DeserializeTo(typeof(CreateAccountResponse), DeserializeAs.Json);
        }

        [Then(@"the account should be added for the user")]
        public void ThenTheAccountShouldBeAddedForTheUser()
        {
            response = Given().ContentType(contentType).Get(baseurl + "/api/users/" + userId.Id + "/accounts/" + createAccountResponse.AccountId).Log(ResponseLogLevel.All);
            account = (Account)response.DeserializeTo(typeof(Account), DeserializeAs.Json);
            Assert.IsTrue(account.balance == 100);
        }

        [When(@"I make a (.*) of (\d+)$")]
        public void GivenIMakeATransactionOf(string action, int p0)
        {
            payload = new UpdateRequest
            {
                Amount = p0
            };
            action = action.ToLower().Trim();
            action = action.Equals("deposit") ? "deposit" : (action.Contains("withdraw") ? "withdraw" : "Invalid action");
            if (action.Equals("Invalid action")) {
                Assert.Fail("Mention action as 'deposit' or 'withdraw'");
            }
            ExecutableRequest er = Given().ContentType(contentType).Body(payload);
            response = action.Equals("deposit") ? 
                er.Put(baseurl + "/api/users/" + userId.Id + "/accounts/" + account.accountId + "/" + action).Log(ResponseLogLevel.All) :
                (   action.Equals("withdraw") ?
                    er.Put(baseurl + "/api/users/" + userId.Id + "/accounts/" + account.accountId + "/" + action).Log(ResponseLogLevel.All) :
                    null);
            try
            {
                msg = (Message)response.DeserializeTo(typeof(Message), DeserializeAs.Json);
            }catch (Exception ex) { }
        }

        [When(@"I make a withdrawal of (\d+) percent$")]
        public void WhenIMakeAWithdrawalOf(int p0)
        {
            payload = new UpdateRequest
            {
                Amount = account.balance * ((decimal)p0 / 100)
            };
            ExecutableRequest er = Given().ContentType(contentType).Body(payload);
            response = er.Put(baseurl + "/api/users/" + userId.Id + "/accounts/" + account.accountId + "/withdraw").Log(ResponseLogLevel.All);
            msg = (Message)response.DeserializeTo(typeof(Message), DeserializeAs.Json);
        }

        [When(@"I set the account balance to (\d+)")]
        public void GivenISetTheAccountBalanceTo(int p0)
        {
            decimal difference;
            int temp;
            if (account.balance<p0)
            {
                difference = (decimal)p0 - account.balance;
                while (difference >= 10000)
                {
                    GivenIMakeATransactionOf("deposit", 10000);
                    difference = difference - (decimal)10000;
                }
                if (difference > 0) GivenIMakeATransactionOf("deposit", (int)difference);
            }
            else if (account.balance > (decimal)p0)
            {
                difference = account.balance - (decimal)p0;
                while (difference > (decimal)((int)account.balance * 0.9) && difference > 100)
                {
                    temp = (int)((int)account.balance * 0.9);
                    GivenIMakeATransactionOf("withdraw", temp);
                    difference = difference - (decimal)temp;
                }
                if(difference>0) GivenIMakeATransactionOf("withdraw", (int)difference);
            }
            response = Given().ContentType(contentType).Get(baseurl + "/api/users/" + userId.Id + "/accounts/" + account.accountId).Log(ResponseLogLevel.All);
            account = (Account)response.DeserializeTo(typeof(Account), DeserializeAs.Json);

        }


        [Then(@"the user account should be (.*) with (.*)")]
        public void GivenTheUserAccountShouldBeUpdateWith(string action, int p0)
        {
            decimal? newBalance = 0 ;
            action = action.ToLower().Trim();
            newBalance = action.Equals("updated") ? account.balance + p0 : (action.Equals("deducted") ? account.balance - p0 : null );
            if (newBalance == null)
            {
                Assert.Fail($"Please mention the action as 'updated' or 'deducted'");
            } else if (newBalance <= 0)
            {
                Assert.Fail($"Make sure the account is having +ive value after action. Current balance is {account.balance}");
            }
            response = Given().ContentType(contentType).Get(baseurl + "/api/users/" + userId.Id + "/accounts/" + account.accountId).Log(ResponseLogLevel.All);
            account = (Account)response.DeserializeTo(typeof(Account), DeserializeAs.Json);
            Assert.AreEqual(account.balance, newBalance, $"New Balance {account.balance} should be {newBalance} after deposit");
        }

        [Then(@"the user should get a message containing ""([^""]*)""")]
        public void ThenTheUserShouldGetAMessageContaining(string p0)
        {
            Assert.True(msg.msg.Contains(p0), "'" + msg.msg + "' does not contain a response containing '" + p0 +"'");
        }

        [When(@"I delete the account for that user")]
        public void WhenIDeleteAnAccountForThatUser()
        {
            response = Given().ContentType(contentType).Delete(baseurl + "/api/users/" + userId.Id + "/accounts/" + account.accountId).Log(ResponseLogLevel.All);
            msg = (Message)response.DeserializeTo(typeof(Message), DeserializeAs.Json);
        }

        [When(@"I delete all accounts for that user")]
        public void WhenIDeleteAllAccountsForThatUser()
        {
            response = Given().ContentType(contentType).Get(baseurl + "/api/users/" + userId.Id + "/accounts").Log(ResponseLogLevel.All);
            accounts = (List<Account>)response.DeserializeTo(typeof(List<Account>), DeserializeAs.Json);
            if (accounts.Count > 0) {
                foreach (Account acc in accounts)
                {
                    account = acc;
                    WhenIDeleteAnAccountForThatUser();
                }
            }
        }

        [Given(@"I delete all accounts for that user")]
        public void GivenIDeleteAllAccountsForThatUser()
        {
            WhenIDeleteAllAccountsForThatUser();
        }



        [When(@"I delete the user")]
        public void WhenIDeleteTheUser()
        {
            response = Given().ContentType(contentType).Delete(baseurl + "/api/users/" + userId.Id).Log(ResponseLogLevel.All);
            msg = (Message)response.DeserializeTo(typeof(Message), DeserializeAs.Json);
        }


        [Then(@"finding the not existing account should give message ""([^""]*)""")]
        public void ThenFindTheNotExistingAccountShouldGiveMessage(string p0)
        {
            p0 = p0.Replace("<accountId>", account.accountId);
            p0 = p0.Replace("<userId>", userId.Id);
            response = Given().ContentType(contentType).Get(baseurl + "/api/users/" + userId.Id + "/accounts/" + account.accountId).Log(ResponseLogLevel.All);
            msg = (Message)response.DeserializeTo(typeof(Message), DeserializeAs.Json);
            Assert.True(msg.msg.Equals(p0), "'" + msg.msg + "' does not have a response '" + p0 + "'");
        }

        [Then(@"finding the not existing user should give message ""([^""]*)""")]
        public void ThenFindTheNotExistingUserShouldGiveMessage(string p0)
        {
            p0 = p0.Replace("<userId>", userId.Id);
            response = Given().ContentType(contentType).Get(baseurl + "/api/users/" + userId.Id).Log(ResponseLogLevel.All);
            msg = (Message)response.DeserializeTo(typeof(Message), DeserializeAs.Json);
            Assert.True(msg.msg.Equals(p0), "'" + msg.msg + "' does not have a response '" + p0 + "'");
        }
    }
}
