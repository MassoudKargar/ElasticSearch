using ElasticSearch.Models;

using Microsoft.AspNetCore.Mvc;

using Nest;

using Newtonsoft.Json;

namespace ElasticSearch.Controllers
{
    public class ArticleController : Controller
    {
        private readonly IElasticClient _elasticClient;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ArticleController(IElasticClient elasticClient, IWebHostEnvironment hostingEnvironment)
        {
            _elasticClient = elasticClient;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public ActionResult Index(string keyword)
        {
            var articleList = new List<ArticleModel>();
            if (!string.IsNullOrEmpty(keyword))
            {
                articleList = GetSearch(keyword).ToList();
            }

            return View(articleList.AsEnumerable());
        }

        public IList<ArticleModel> GetSearch(string keyword)
        {
            var result = _elasticClient.SearchAsync<ArticleModel>(
                s => s.Query(
                    q => q.QueryString(
                        d => d.Query('*' + keyword + '*')
                    )).Size(5000));

            var finalResult = result;
            var finalContent = finalResult.Result.Documents.ToList();
            return finalContent;
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ArticleModel model)
        {
            try
            {
                var article = new ArticleModel()
                {
                    Id = 1,
                    Title = model.Title,
                    Link = model.Link,
                    Author = model.Author,
                    AuthorLink = model.AuthorLink,
                    PublishedDate = DateTime.Now
                };

                await _elasticClient.IndexDocumentAsync(article);
                model = new ArticleModel();
            }
            catch (Exception ex)
            {
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(ArticleModel model)
        {
            try
            {
                var article = new ArticleModel()
                {
                    Id = 1,
                    Title = model.Title,
                    Link = model.Link,
                    Author = model.Author,
                    AuthorLink = model.AuthorLink,
                    PublishedDate = DateTime.Now
                };

                await _elasticClient.DeleteAsync<ArticleModel>(article);
                model = new ArticleModel();
            }
            catch (Exception ex)
            {
            }

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import()
        {
            try
            {
                var response = await _elasticClient.BulkAsync(f =>
                f.IndexMany<ArticleModel>(JsonConvert.DeserializeObject<List<ArticleModel>>(
                    System.IO.File.ReadAllText(Path.Combine(_hostingEnvironment.ContentRootPath, "articles.json")))), CancellationToken.None);
            }
            catch (Exception ex)
            {
            }

            return await Task.FromResult(RedirectToAction("Index"));
        }

    }
}