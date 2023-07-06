using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace fileServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class fileServerController: ControllerBase
    {

        private string definePathName(string directory)
        {
            string path = "";
            switch (directory?.ToLower().Trim())
            {
                case "rafael":
                    path = "/rafael";
                    break;
                default:
                    path = "/public";
                    break;
            }
            return "/files/" + path;
        }

        // GET api/file/upload
        /// <summary>
        /// Receive form data containing a file, save file locally with a unique id as the name, and return the unique id
        /// </summary>
        /// <param name="file">Received IFormFile file</param>
        /// <returns>IAction Result</returns>
        [HttpPost("upload")]
        [EnableCors("MyPolicy")]
        public async Task<IActionResult> Upload(IFormFile file, string? directory)
        {

            string guacHomePath = Path.Combine(Directory.GetCurrentDirectory() + definePathName(directory));
            // create the new file name consisting of the current time plus a GUID
            string newFileName = Path.GetFileNameWithoutExtension(file.FileName) + "_" + DateTime.Now.Ticks + "_" + Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            // Verify the home-guac directory exists, and combine the home-guac directory with the new file name
            Directory.CreateDirectory(guacHomePath);
            var filePath = Path.Combine(guacHomePath, newFileName);

            // Create a new file in the home-guac directory with the newly generated file name
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                //copy the contents of the received file to the newly created local file 
                await file.CopyToAsync(stream);
            }
            // return the file name for the locally stored file
            return Ok(newFileName);
        }

        // GET api/file/downlaod
        /// <summary>
        /// Return a locally stored file based on id to the requesting client
        /// </summary>
        /// <param name="id">unique identifier for the requested file</param>
        /// <returns>an IAction Result</returns>
        [HttpGet("download")]
        public async Task<IActionResult> Download(string id, string? directory)
        {
            string guacHomePath = Path.Combine(Directory.GetCurrentDirectory() + definePathName(directory));
            string path = Path.Combine(guacHomePath, id);

            if (System.IO.File.Exists(path))
            {
                // Get all bytes of the file and return the file with the specified file contents 
                byte[] b = await System.IO.File.ReadAllBytesAsync(path);
                return File(b, "application/octet-stream", id);
            }

            else
            {
                // return error if file not found
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }

        [HttpGet("list")]
        public async Task<IActionResult> getAllFiles(string directory)
        {
            string guacHomePath = Path.Combine(Directory.GetCurrentDirectory() + definePathName(directory));

            DirectoryInfo d = new DirectoryInfo(guacHomePath); //Assuming Test is your Folder

            FileInfo[] Files = d.GetFiles();
            List<string> fileNames = new List<string>();
            if (Files.Length > 0)
            {
                foreach(FileInfo file in Files)
                {
                    fileNames.Add(file.Name);
                }
                return Ok(fileNames);
            }
            return NotFound("Não existem arquivos no diretório");

        }

        [HttpDelete("delete")]
        public async Task<IActionResult> deleteFile(string id, string? directory)
        {
            string guacHomePath = Path.Combine(Directory.GetCurrentDirectory() + definePathName(directory));
            DirectoryInfo d = new DirectoryInfo(guacHomePath); //Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles();

            foreach (FileInfo file in Files)
            {
                string fileName = file.Name;
                if(file.Name == id)
                {
                    file.Delete();
                    return Ok();
                }
            }
            return NotFound();
        }
    }
}
