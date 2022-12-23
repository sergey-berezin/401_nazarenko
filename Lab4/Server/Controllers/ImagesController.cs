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

        [HttpGet]
        async public Task<ActionResult<int[]?>> GetIds()
        {
            int[]? ids;
            try
            {
                ids = await db.GetIds();
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
            if(ids == null)
            {
                return StatusCode(500, "Something went wrong during getting id's");
            }
            return ids;
        }

        [HttpDelete]
        async public Task<ActionResult> DeleteImgs()
        {
            try
            {
                await db.DeleteImages();
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
            return StatusCode(200, "Images deleted");

        }

        [HttpDelete("{id}")]
        async public Task<ActionResult> DeleteImage(int id)
        {
            try
            { 
                await db.DeleteImage(id);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
            return StatusCode(200, "Image deleted");

        }

        [HttpPost]
        async public Task<ActionResult<int>> PostImg(Image img)
        {
            int i = 0;
            try
            {
                i = await db.AddImage(img);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
            if(i == -1)
            {
                StatusCode(500, $"Can't ad image to database");
            }
            return i;
        }


        [HttpGet("{id}")]
        async public Task<ActionResult<Image?>> TryGetImage(int id)
        {
            Image? img;
            try
            {
                img = await db.TryGetImage(id);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
            if (img is null)
            {
                return StatusCode(404, $"Image id={id} not found");
            }
            return img;
        }
    }
}
