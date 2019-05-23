using BangazonAPI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TestStudentExercisesAPI;
using Xunit;

namespace TestBangazonAPI
{
    public class PaymentTypeTest
    {
        // Create a new paymentType in the db and make sure we get a 200 OK status code back

        public async Task<PaymentType> createPaymentType(HttpClient client)
        {
            PaymentType paymentType = new PaymentType
            {
                AcctNumber = 123456,
                Name = "test payment type",
                CustomerId = 1,
                Archived = false


            };
            string paymentTypeAsJSON = JsonConvert.SerializeObject(paymentType);


            HttpResponseMessage response = await client.PostAsync(
                "api/PaymentType",
                new StringContent(paymentTypeAsJSON, Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            PaymentType newPaymentType = JsonConvert.DeserializeObject<PaymentType>(responseBody);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return newPaymentType;

        }

        // Delete a paymentType in the database and make sure we get a no content status code back
        public async Task DeletePaymentType(PaymentType paymentType, HttpClient client)
        {
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/PaymentType/{paymentType.Id}?Delete=true");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        }


        [Fact]
        public async Task Test_Get_All_PaymentTypes()
        {
            // Use the http client
            using (HttpClient client = new APIClientProvider().Client)
            {

                // Call the route to get all our paymentTypes; wait for a response object
                HttpResponseMessage response = await client.GetAsync("api/paymentType");

                // Make sure that a response comes back at all
                response.EnsureSuccessStatusCode();

                // Read the response body as JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert the JSON to a list of paymentType instances
                List<PaymentType> paymentTypeList = JsonConvert.DeserializeObject<List<PaymentType>>(responseBody);

                // Did we get back a 200 OK status code?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                // Are there any paymentTypes in the list?
                Assert.True(paymentTypeList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_PaymentType()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new paymentType
                PaymentType newPaymentType = await createPaymentType(client);

                // Try to get that paymentType from the database
                HttpResponseMessage response = await client.GetAsync($"api/paymentType/{newPaymentType.Id}");

                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                PaymentType paymentType = JsonConvert.DeserializeObject<PaymentType>(responseBody);

                // Did we get back what we expected to get back? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("test payment type", newPaymentType.Name);


                // Clean up after ourselves- delete paymentType!
                DeletePaymentType(newPaymentType, client);
            }
        }

        [Fact]
        public async Task Test_Get_NonExitant_PaymentType_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to get a paymentType with an enormously huge Id
                HttpResponseMessage response = await client.GetAsync("api/paymentType/999999999");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }


        [Fact]
        public async Task Test_Create_And_Delete_PaymentType()
        {
            using (var client = new APIClientProvider().Client)
            {

                // Create a new PaymentType
                PaymentType newPaymentType = await createPaymentType(client);

                // Make sure the info checks out
                Assert.Equal("test payment type", newPaymentType.Name);



                // Clean up after ourselves - delete PaymentType!
                DeletePaymentType(newPaymentType, client);
            }
        }

        [Fact]
        public async Task Test_Delete_NonExistent_PaymentType_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Try to delete an Id that shouldn't exist in the DB
                HttpResponseMessage deleteResponse = await client.DeleteAsync("/api/paymentType/600000");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Modify_PaymentType()
        {

            // We're going to change a paymentType's name! This is their new name.
            string newName = "Magic Internet Money";

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new paymentType
                PaymentType newPaymentType = await createPaymentType(client);

                // Change their first name
                newPaymentType.Name = newName;

                // Convert them to JSON
                string modifiedPaymentTypeAsJSON = JsonConvert.SerializeObject(newPaymentType);

                // Make a PUT request with the new info
                HttpResponseMessage response = await client.PutAsync(
                    $"api/PaymentType/{newPaymentType.Id}",
                    new StringContent(modifiedPaymentTypeAsJSON, Encoding.UTF8, "application/json")
                );


                response.EnsureSuccessStatusCode();

                // Convert the response to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // We should have gotten a no content status code
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                // Try to GET the paymentType we just edited
                HttpResponseMessage getPaymentType = await client.GetAsync($"api/paymentType/{newPaymentType.Id}");
                getPaymentType.EnsureSuccessStatusCode();

                string getPaymentTypeBody = await getPaymentType.Content.ReadAsStringAsync();
                PaymentType modifiedPaymentType = JsonConvert.DeserializeObject<PaymentType>(getPaymentTypeBody);

                Assert.Equal(HttpStatusCode.OK, getPaymentType.StatusCode);

                // Make sure the name was in fact updated
                Assert.Equal(newName, modifiedPaymentType.Name);

                // Clean up after ourselves- delete it
                _ = DeletePaymentType(modifiedPaymentType, client);
            }
        }
    }
}
