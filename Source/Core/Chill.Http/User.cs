namespace Chill.Http
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    using AppFunc = System.Func<
        System.Collections.Generic.IDictionary<string, object>,
        System.Threading.Tasks.Task
    >;

    public class AuthenticateAction : IUserAction
    {
        private readonly HttpClient _client;
        private readonly User _user;
        private readonly HttpClientBuilder _httpClientBuilder;
        private readonly Scenario _scenario;

        public AuthenticateAction(User user, HttpClientBuilder httpClientBuilder)
        {
            _user = user;
            _httpClientBuilder = httpClientBuilder;
        }

        public string Message
        {
            get { return string.Format("User {0} logs in", _user); }
        }

        public IEnumerable<ResponseAction> ResultActions
        {
            get { return Enumerable.Empty<ResponseAction>(); }
        }

        public Task Execute()
        {
            return _httpClientBuilder.Authenticate(_user);
        }

    }

    

    public class User
    {
        private readonly string _name;
        private HttpClientBuilder _builder;

        public User(string name)
        {
            _name = name;
        }

        public HttpClient Client { get; private set; }
        public Scenario Scenario { get; set; }

        public string Name
        {
            get { return _name; }
        }

        public AuthenticateAction LogsIn()
        {
            return new AuthenticateAction(this, _builder);
        }

        public override string ToString()
        {
            return string.Format("{0}", Name);
        }

        public void Initialize(HttpClientBuilder builder)
        {
            _builder = builder;
            Client = builder.Build(this);
        }

        //public HttpBasedUserAction Does(object command, Guid? commandId = null, bool checkStatusCodeIsSuccess = true)
        //{
        //    if(command == null)
        //    {
        //        throw new ArgumentNullException("command");
        //    }
        //    var message = string.Format("User {0} executes the command '{2}':'{1}'", _name, command.GetType().Name, command);

        //    var requestMessage = CommandClient.CreatePutCommandRequest(command, commandId ?? Guid.NewGuid(), _commandPath);

        //    return new HttpBasedUserAction(message, this, requestMessage, checkStatusCodeIsSuccess);
        //}

        //public HttpBasedUserAction<TResult> Queries<TResult>(object query, bool checkStatusCodeIsSuccess = true)
        //{
        //    if(query == null)
        //    {
        //        throw new ArgumentNullException("query");
        //    }

        //    var message = string.Format("User {0} queries '{2}':'{1}'", _name, query.GetType().Name, query);

        //    var queryString = GetQueryString(query);
        //    var url = _queryPath + "/" + query.GetType().Name + "?" + queryString;

        //    var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
        //    requestMessage.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        //    return new HttpBasedUserAction<TResult>(message,
        //        this,
        //        requestMessage,
        //        Deserialize<TResult>,
        //        checkStatusCodeIsSuccess);
        //}

        private TResult Deserialize<TResult>(HttpResponseMessage response)
        {
            Stream responseStream = response.Content.ReadAsStreamAsync().Result;

            if(response.Content.Headers.ContentEncoding.Any(x => x == "gzip"))
            {
                responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
            }
            
            using(var reader = new StreamReader(responseStream))
            {
                string responseString = reader.ReadToEndAsync().Result;
                var serializer = new JsonSerializer();
                try
                {
                    return (TResult)JsonConvert.DeserializeObject(responseString, typeof(TResult));
                    //return
                    //    (TResult)
                    //        serializer.Deserialize(reader, typeof(TResult));
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        string.Format("Failed to deserialize to '{1}' the response: '{0}'", response.Content.ReadAsStringAsync().Result, typeof(TResult).Name), ex);
                }
                
            }

        }

        public string GetQueryString(object obj)
        {
            var properties = from p in obj.GetType().GetProperties()
                where p.GetValue(obj, null) != null
                select p.Name + "=" + Uri.EscapeDataString(p.GetValue(obj, null).ToString());

            return String.Join("&", properties.ToArray());
        }

        // Usage:
        public HttpBasedUserAction Posts(string url, IRequestMessage data, bool checkStatusCodeIsSuccess = true)
        {
            var message = string.Format("User {0} posts {1}:{2}", Name, data.GetType().Name, data);
            return new HttpBasedUserAction(message, this, data.Build(url, HttpMethod.Post), checkStatusCodeIsSuccess);
        }

        public HttpBasedUserAction Gets(string url, bool checkStatusCodeIsSuccess = true)
        {
            var message = string.Format("User {0} gets {1}", Name, url);
            return new HttpBasedUserAction(message, this, new HttpRequestMessage(HttpMethod.Get, url), checkStatusCodeIsSuccess);
        }
    }

    public interface IRequestMessage
    {
        HttpRequestMessage Build(string message, HttpMethod method);
    }

    public class FileToUpload : IRequestMessage
    {
        public FileToUpload(byte[] content, string contentType, string fileName, string fileId = null)
        {
            Content = content;
            ContentType = contentType;
            FileName = fileName;
            FileId = fileId;
        }

        public string FileName { get; set; }
        public string FileId { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }

        public HttpRequestMessage Build(string url, HttpMethod method)
        {
            var message = new HttpRequestMessage(method, url)
            {
                Content = new ByteArrayContent(Content)
            };

            message.Content.Headers.ContentType = new MediaTypeHeaderValue(ContentType);
            return message;
        }

        public override string ToString()
        {
            return string.Format("{0} {2} ({1} bytes)", FileName, Content.Length, FileId);
        }
    }

    public class MultiPartFileUpload : IRequestMessage
    {
        private readonly FileToUpload[] _filesToUpload;

        public MultiPartFileUpload(params FileToUpload[] filesToUpload)
        {
            _filesToUpload = filesToUpload;
        }

        public HttpRequestMessage Build(string url, HttpMethod method)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, url);
            var content = new MultipartFormDataContent();
            foreach(var file in _filesToUpload)
            {
                var byteArrayContent = new ByteArrayContent(file.Content);
                byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                content.Add(byteArrayContent, file.FileId ?? file.FileName, file.FileName);
            }
            message.Content = content;
            return message;
        }

        public override string ToString()
        {
            return string.Format("MultiPartFormPost: {0}", string.Join(",", _filesToUpload.Select(x => x.ToString())));
        }
    }
}