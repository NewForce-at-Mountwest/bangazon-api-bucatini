using Newtonsoft.Json;
using BangazonAPI.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using TestStudentExercisesAPI;

namespace TestBangazonAPI
{

    public class ProductTypeTest
    {


        // Create a new Product in the db and make sure we get a 200 OK status code back
        public async Task<ProductType> createType(HttpClient client)
        {
            ProductType Drinks = new ProductType
            {
                Name = "Drinks"
                
            };
            string ProductTypeAsJSON = JsonConvert.SerializeObject(Drinks);


            HttpResponseMessage response = await client.PostAsync(
                "api/ProductType",
                new StringContent(ProductTypeAsJSON, Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            ProductType Pepsi = JsonConvert.DeserializeObject<ProductType>(responseBody);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return Pepsi;

        }

        // Delete a Product in the database and make sure we get a no content status code back
        public async Task deleteDrink(Product newDrink, HttpClient client)
        {
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/Product/{newDrink.Id}?delete=True");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        }


        [Fact]
        public async Task Test_Get_All_Products()
        {
            // Use the http client
            using (HttpClient client = new APIClientProvider().Client)
            {

                // Call the route to get all our Products; wait for a response object
                HttpResponseMessage response = await client.GetAsync("api/Product");


                response.EnsureSuccessStatusCode();

                // Read the response body as JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert the JSON to a list of Product instances
                List<Product> ProductList = JsonConvert.DeserializeObject<List<Product>>(responseBody);

                // Did we get back a 200 OK status code?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                // Are there any Products in the list?
                Assert.True(ProductList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_Product()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new Product
                Product newRedMug = await createMug(client);

                // Try to get that Product from the database
                HttpResponseMessage response = await client.GetAsync($"api/Product/{newRedMug.Id}");

                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Product Product = JsonConvert.DeserializeObject<Product>(responseBody);

                // Did we get back what we expected to get back? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Red Mug", newRedMug.Title);
                Assert.Equal(5000, newRedMug.Price);

                // Clean up after ourselves- delete newRedMug!
                deleteRedMug(newRedMug, client);
            }
        }

        [Fact]
        public async Task Test_Get_NonExistant_Product_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to get a Product with an enormously huge Id
                HttpResponseMessage response = await client.GetAsync("api/Product/999999999");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }


        [Fact]
        public async Task Test_Create_And_Delete_Product()
        {
            using (var client = new APIClientProvider().Client)
            {

                // Create a new David
                Product redMug = await createMug(client);

                // Make sure his info checks out
                Assert.Equal("Red Mug", redMug.Title);
                Assert.Equal(5000, redMug.Price);


                // Clean up after ourselves - delete redMug!
                deleteRedMug(redMug, client);
            }
        }

        [Fact]
        public async Task Test_Delete_NonExistent_Product_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Try to delete an Id that shouldn't exist in the DB
                HttpResponseMessage deleteResponse = await client.DeleteAsync("/api/Products/600000");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Modify_Product()
        {

            // We're going to change a Product's name! This is their new name.
            string newTitle = "Super cool mug";

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new Product
                Product brandNewRedMug = await createMug(client);

                // Change their first name
                brandNewRedMug.Title = newTitle;

                // Convert them to JSON
                string modifiedMugAsJSON = JsonConvert.SerializeObject(brandNewRedMug);

                // Make a PUT request with the new info
                HttpResponseMessage response = await client.PutAsync(
                    $"api/Product/{brandNewRedMug.Id}",
                    new StringContent(modifiedMugAsJSON, Encoding.UTF8, "application/json")
                );


                response.EnsureSuccessStatusCode();

                // Convert the response to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // We should have gotten a no content status code
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                // Try to GET the Product we just edited
                HttpResponseMessage getRedMug = await client.GetAsync($"api/Product/{brandNewRedMug.Id}");
                getRedMug.EnsureSuccessStatusCode();

                string getMugInfo = await getRedMug.Content.ReadAsStringAsync();
                Product modifiedRedMug = JsonConvert.DeserializeObject<Product>(getMugInfo);

                Assert.Equal(HttpStatusCode.OK, getRedMug.StatusCode);

                // Make sure his name was in fact updated
                Assert.Equal(newTitle, modifiedRedMug.Title);

                // Clean up after ourselves- delete it
                deleteRedMug(modifiedRedMug, client);
            }
        }
    }
}