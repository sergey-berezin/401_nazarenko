using Microsoft.AspNetCore.Mvc;
using Contracts;


namespace Server
{


    [ApiController]
    [Route("/[controller]")]
    public class ImagesController : Controller
    {

        private IImageDataBase db;

        public ImagesController(IImageDataBase db)
        {
            this.db = db;
        }

        [HttpGet] //По этому запросу будет выдаваться нужный JSON
        async public Task<ActionResult<List<List<Image_get>>>> GetImages()
        {
            List<List<Image_get>> ids = new();
            try
            {
                ids = await db.GetDB();
            }
            catch (Exception e)
            {
                return StatusCode(400, "Something went wrong during getting images");
            }
            if (ids.Count == 0)
            {
                return StatusCode(400, "Something went wrong during getting images");
            }
            return ids;
        }


        [HttpDelete("{id}")] //Удаляет изображение из базы данных с нужным id
        async public Task<ActionResult> DeleteImage(int id)
        {
            try
            {
                await db.DeleteImage(id);
            }
            catch (Exception e)
            {
                return StatusCode(400, "Can't delete Image");
            }
            return StatusCode(200, "Image deleted");

        }



        [HttpDelete] //Удаляет базу данных
        async public Task<ActionResult> DeleteImgs()
        {
            try
            {
                await db.DeleteImages();
            }
            catch (Exception e)
            {
                return StatusCode(400, "Can't delete Images");
            }
            return StatusCode(200, "Images deleted");
        }


        [HttpPost] //Для web не нужно - моя забота, отправляет изображение на сервер
        async public Task<ActionResult<int>> PostImg(Image_post img)
        {
            int i = 0;
            try
            {
                i = await db.AddImage(img);
            }
            catch (Exception e)
            {
                return StatusCode(400, "Can't add image to database");
            }
            if (i == -1)
            {
                return StatusCode(400, $"Can't add image to database");
            }
            return i;
        }

        [Route("/")]
        [HttpGet] // Открываем домашнюю страницу
        public ContentResult Index()
        {
            var fileContents = System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "web_console.html"));
            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = 200,
                Content = fileContents,
            };
        }

    }
}
