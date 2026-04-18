using Exam_Maya_18._04.DTOs;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace Exam_Maya_18_04
{
    public class Tests
    {
        private RestClient client;
        public static string CreatedMovieId;

        [OneTimeSetUp] 
        public void Setup()
        {
            string jwtToken = GetJwtToken("maya1@abv.com", "123456");
            RestClientOptions options = new RestClientOptions("http://144.91.123.158:5000/")
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };
            client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            RestClient client = new RestClient("http://144.91.123.158:5000");
            RestRequest request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });
            RestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
            }
        }

        [Order(1)]
        [Test]
        public void CreateNewMovie_Success()
        {
            MovieDTO movie = new MovieDTO();
            {

                movie.Title = "OldMovie5";
                movie.Description = "drama5";

            };

            RestRequest request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movie);
            RestResponse response = client.Execute(request);

           
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

         
            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content,new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });


          
            Assert.That(readyResponse, Is.Not.Null);

            
            Assert.That(readyResponse.Msg, Is.EqualTo("Movie created successfully!"));

            
            Assert.That(readyResponse.Movie, Is.Not.Null);
            Assert.That(readyResponse.Movie.Id, Is.Not.Null.And.Not.Empty);


            CreatedMovieId = readyResponse.Movie.Id;

           
        }

        [Order(2)]
        [Test]

        public void EditMovie_Success()
        {
           
            MovieDTO editedMovie = new MovieDTO
            {
                Title = "OldMovie5",
                Description = "drama5"
            };

            RestRequest request = new RestRequest("/api/Movie/Edit", Method.Put);

           
            request.AddQueryParameter("movieId", CreatedMovieId);

            request.AddJsonBody(editedMovie);

           
            RestResponse response = client.Execute(request);

          
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(
                response.Content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            
            Assert.That(readyResponse, Is.Not.Null);
            Assert.That(readyResponse.Msg, Is.EqualTo("Movie edited successfully!"));
        }


        [Order(3)]
        [Test]
        public void GetAllMovies_Success()
        {
          
            RestRequest request = new RestRequest("/api/Catalog/All", Method.Get);

           
            RestResponse response = client.Execute(request);

           
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            
            List<MovieDTO> movies = JsonSerializer.Deserialize<List<MovieDTO>>(
                response.Content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            
            Assert.That(movies, Is.Not.Null);
            Assert.That(movies.Count, Is.GreaterThan(0));
        }


        [Order(4)]
        [Test]
        public void DeleteMovie_Success()
        {
            
            RestRequest request = new RestRequest("/api/Movie/Delete", Method.Delete);


            request.AddQueryParameter("movieId", CreatedMovieId);


            RestResponse response = client.Execute(request);

            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            
            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(
                response.Content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

           
            Assert.That(readyResponse, Is.Not.Null);
            Assert.That(readyResponse.Msg, Is.EqualTo("Movie deleted successfully!"));
        }



        [Order(5)]
        [Test]
        public void CreateMovie_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            
            MovieDTO movie = new MovieDTO
            {
                Title = null,
                Description = null
            };

            RestRequest request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movie);

            
            RestResponse response = client.Execute(request);

            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }



        [Order(6)]
        [Test]
        public void EditNonExistingMovie_ShouldReturnBadRequest()
        {

            string nonExistingMovieId = "00000000-0000-0000-0000-000000000000";

            MovieDTO editedMovie = new MovieDTO
            {
                Title = "DoesNotExist",
                Description = "DoesNotExist"
            };

            RestRequest request = new RestRequest("/api/Movie/Edit", Method.Put);

            request.AddQueryParameter("movieId", nonExistingMovieId);
            request.AddJsonBody(editedMovie);

            
            RestResponse response = client.Execute(request);

            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(
                response.Content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            
            Assert.That(
                readyResponse.Msg,
                Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!")
            );
        }



        [Order(7)]
        [Test]
        public void DeleteNonExistingMovie_ShouldReturnBadRequest()
        {
          
            string nonExistingMovieId = "00000000-0000-0000-0000-000000000000";

            RestRequest request = new RestRequest("/api/Movie/Delete", Method.Delete);

            
            request.AddQueryParameter("movieId", nonExistingMovieId);

            
            RestResponse response = client.Execute(request);

           
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(
                response.Content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            
            Assert.That(
                readyResponse.Msg,
                Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!")
            );
        }


        [OneTimeTearDown] 
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}