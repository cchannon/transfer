using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace AWS_POC
{
    class Program
    {
        static string _bucketName = "medalliapoc";
        static string _key = Guid.NewGuid().ToString();
        static IAmazonS3 _client;
        static string _bodyText = "";
        //static string _responseBody = "";

        static void Main(string[] args)
        {
            Console.WriteLine("Medallia Integration - S3 Proof Of Concept");
            Console.WriteLine($"currently reading/writing at: {Directory.GetCurrentDirectory()}");
            Console.ReadLine();
            if (CheckRequiredFields())
            {
                using (_client = new AmazonS3Client())
                {
                    Console.WriteLine("Listing out Buckets...");
                    PrintBuckets();
                    Console.WriteLine("Reading CSV from disk...");
                    ReadCSV();
                    Console.WriteLine("Uploading CSV to S3...");
                    UploadCSV();
                    Console.WriteLine("Retrieving CSV from S3...");
                    PullCSV();
                    Console.WriteLine("Deleting CSV from S3...");
                    DeleteCSV();
                    //Console.WriteLine("Writing CSV to Disk...");
                    //WriteCSV();
                }
            }
            Console.WriteLine("Press any key to close");
            Console.ReadLine();
        }

        private static void DeleteCSV()
        {
            try
            {
                DeleteObjectRequest request = new DeleteObjectRequest()
                {
                    BucketName = _bucketName,
                    Key = _key
                };

                _client.DeleteObject(request);
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Please check the provided AWS Credentials.");
                    Console.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
                }
                else
                {
                    Console.WriteLine("An error occurred with the message '{0}' when deleting an object", amazonS3Exception.Message);
                }
            }
        }

        //private static void WriteCSV()
        //{
        //    File.WriteAllText(Directory.GetCurrentDirectory() + $"\testcopy.csv", _responseBody);
        //}

        private static void PullCSV()
        {
            try
            {
                GetObjectRequest request = new GetObjectRequest()
                {
                    BucketName = _bucketName,
                    Key = _key
                };

                using (GetObjectResponse response = _client.GetObject(request))
                {
                    string title = response.Metadata["x-amz-meta-title"];
                    Console.WriteLine("The object's title is {0}", title);
                    string dest = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), _key);
                    if (!File.Exists(dest))
                    {
                        response.WriteResponseStreamToFile(dest);
                    }
                }
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Please check the provided AWS Credentials.");
                    Console.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
                }
                else
                {
                    Console.WriteLine("An error occurred with the message '{0}' when reading an object", amazonS3Exception.Message);
                }
            }
        }

        private static void ReadCSV()
        {
            _bodyText = File.ReadAllText(Directory.GetCurrentDirectory() + @"\test.csv");
            Console.WriteLine(_bodyText);
        }

        private static void PrintBuckets()
        {
            try
            {
                Console.WriteLine("Retrieving list of buckets...");
                ListBucketsResponse response = _client.ListBuckets();
                foreach (S3Bucket bucket in response.Buckets)
                {
                    Console.WriteLine("Found bucket with name: {0}", bucket.BucketName);
                }
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Please check the provided AWS Credentials.");
                    Console.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
                }
                else
                {
                    Console.WriteLine("An Error, number {0}, occurred when listing buckets with the message '{1}", amazonS3Exception.ErrorCode, amazonS3Exception.Message);
                }
            }
        }

        static void UploadCSV()
        {
            try
            {
                PutObjectRequest request = new PutObjectRequest()
                {
                    ContentBody = _bodyText,
                    BucketName = _bucketName,
                    Key = _key
                };

                PutObjectResponse response = _client.PutObject(request);

                // put a more complex object with some metadata and http headers.
                //PutObjectRequest titledRequest = new PutObjectRequest()
                //{
                //    BucketName = _bucketName,
                //    Key = _key
                //};
                //titledRequest.Metadata.Add("title", "the title");

                //_client.PutObject(titledRequest);
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Please check the provided AWS Credentials.");
                    Console.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
                }
                else
                {
                    Console.WriteLine("An error occurred with the message '{0}' when writing an object", amazonS3Exception.Message);
                }
            }
        }

        static bool CheckRequiredFields()
        {
            NameValueCollection appConfig = ConfigurationManager.AppSettings;

            if (string.IsNullOrEmpty(appConfig["AWSProfileName"]))
            {
                Console.WriteLine("AWSProfileName was not set in the App.config file.");
                return false;
            }
            if (string.IsNullOrEmpty(_bucketName))
            {
                Console.WriteLine("The variable bucketName is not set.");
                return false;
            }

            return true;
        }
    }
}
