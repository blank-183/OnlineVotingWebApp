using OnlineVotingWebApp.Models;

namespace OnlineVotingWebApp.VoterViewModels
{

    public class VotingFormViewModel
    {
        public List<CandidatePosition> CandidatePositions { get; set; }
        public List<Candidate> Candidates { get; set; }
        public Dictionary<int, int> SelectedCandidateIds { get; set; }
        public string VoterId { get; set; }
    }

}
