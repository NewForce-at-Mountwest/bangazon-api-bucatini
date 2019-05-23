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


        // Create a new ProductType in the db and make sure we get a 200 OK status code back
        public async Task<ProductType> createFurniture(HttpClient client)
        {
            ProductType Furniture = new ProductType
            {
                Name = "Furniture"
                
            };
            string ProductTypeAsJSON = JsonConvert.SerializeObject(Furniture);


            HttpResponseMessage response = await client.PostAsync(
                "api/ProductType",
                new StringContent(ProductTypeAsJSON, Encoding.UTF8, "application/json")
            );

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            ProductType Couch = JsonConvert.DeserializeObject<ProductType>(responseBody);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return Couch;

        }

        // Delete a ProductType in the database and make sure we get a no content status code back
        public async Task deleteFurniture(ProductType newFurniture, HttpClient client)
        {
            HttpResponseMessage deleteResponse = await client.DeleteAsync($"api/ProductType/{newFurniture.Id}?delete=True");
            deleteResponse.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        }


        [Fact]
        public async Task Test_Get_All_ProductTypes()
        {
            // Use the http client
            using (HttpClient client = new APIClientProvider().Client)
            {

                // Call the route to get all our ProductTypes; wait for a response object
                HttpResponseMessage response = await client.GetAsync("api/ProductType");


                response.EnsureSuccessStatusCode();

                // Read the response body as JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert the JSON to a list of ProductType instances
                List<ProductType> ProductTypeList = JsonConvert.DeserializeObject<List<ProductType>>(responseBody);

                // Did we get back a 200 OK status code?
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                // Are there any ProductTypes in the list?
                Assert.True(ProductTypeList.Count > 0);
            }
        }

        [Fact]
        public async Task Test_Get_Single_ProductType()
        {

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new ProductType
                ProductType newFurniture = await createFurniture(client);

                // Try to get that ProductType from the database
                HttpResponseMessage response = await client.GetAsync($"api/ProductType/{newFurniture.Id}");

                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                ProductType ProductType = JsonConvert.DeserializeObject<ProductType>(responseBody);

                // Did we get back what we expected to get back? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Couches", newFurniture.Name);

                // Clean up after ourselves- delete newRedMug!
                deleteFurniture(newFurniture, client);
            }
        }

        [Fact]
        public async Task Test_Get_NonExistant_ProductType_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to get a ProductType with an enormously huge Id
                HttpResponseMessage response = await client.GetAsync("api/ProductType/999999999");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }


        [Fact]
        public async Task Test_Create_And_Delete_ProductType()
        {
            using (var client = new APIClientProvider().Client)
            {

                // Create a new David
                ProductType Couches = await createFurniture(client);

                // Make sure his info checks out
                Assert.Equal("Couches", Couches.Name);
                


                // Clean up after ourselves - delete redMug!
                deleteFurniture(Couches, client);
            }
        }

        [Fact]
        public async Task Test_Delete_NonExistent_ProductType_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Try to delete an Id that shouldn't exist in the DB
                HttpResponseMessage deleteResponse = await client.DeleteAsync("/api/ProductType/600000");
                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Modify_ProductType()
        {

            // We're going to change a Product Type's name! This is their new name.
            string newName = "Chairs";

            using (HttpClient client = new APIClientProvider().Client)
            {

                // Create a new ProductType
                ProductType brandNewFurniture = await createFurniture(client);

                // Change their first name
                brandNewFurniture.Name = newName;

                // Convert them to JSON
                string modifiedFurnitureAsJSON = JsonConvert.SerializeObject(brandNewFurniture);

                // Make a PUT request with the new info
                HttpResponseMessage response = await client.PutAsync(
                    $"api/ProductType/{brandNewFurniture.Id}",
                    new StringContent(modifiedFurnitureAsJSON, Encoding.UTF8, "application/json")
                );


                response.EnsureSuccessStatusCode();

                // Convert the response to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // We should have gotten a no content status code
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                /*
                    GET section
                 */
                // Try to GET the ProductType we just edited
                HttpResponseMessage getNewFurniture = await client.GetAsync($"api/ProductType/{brandNewFurniture.Id}");
                getNewFurniture.EnsureSuccessStatusCode();

                string getFurnitureInfo = await getNewFurniture.Content.ReadAsStringAsync();
                ProductType modifiedFurniture = JsonConvert.DeserializeObject<ProductType>(getFurnitureInfo);

                Assert.Equal(HttpStatusCode.OK, getNewFurniture.StatusCode);

                // Make sure his name was in fact updated
                Assert.Equal(newName, modifiedFurniture.Name);

                // Clean up after ourselves- delete it
                deleteFurniture(modifiedFurniture, client);
            }
        }
    }
}