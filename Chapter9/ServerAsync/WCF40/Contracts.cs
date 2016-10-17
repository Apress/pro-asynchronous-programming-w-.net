using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Repository40;
using System.ServiceModel;

namespace WCF40
{
    [DataContract(Name="Author", Namespace = "")]
    public class AuthorDTO
    {
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }         
    }

    [DataContract(Name="Title", Namespace = "")]
    public class TitleDTO
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public decimal? Price { get; set; }         
    }

    [DataContract(Name = "FullDetails", Namespace = "")]
    public class FullDetails
    {
        [DataMember]
        public List<AuthorDTO> Authors { get; set; }
        [DataMember]
        public List<TitleDTO> Titles { get; set; }
    }


    [ServiceContract]
    interface IGetPubs
    {
        //[OperationContract]
        //List<AuthorDTO> GetAuthors();
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetAuthors(AsyncCallback callback, object state);
        List<AuthorDTO> EndGetAuthors(IAsyncResult iar);

        //[OperationContract]
        //FullDetails GetAuthorsAndTitles();
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetAuthorsAndTitles(AsyncCallback callback, object state);
        FullDetails EndGetAuthorsAndTitles(IAsyncResult iar);
    }
}