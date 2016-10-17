using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.ServiceModel;

namespace WCF45
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
        [OperationContract]
        Task<List<AuthorDTO>> GetAuthors();

        [OperationContract]
        Task<FullDetails> GetAuthorsAndTitles();
    }
}