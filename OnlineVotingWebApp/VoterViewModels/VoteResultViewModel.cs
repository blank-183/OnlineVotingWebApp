using OnlineVotingWebApp.Models;

namespace OnlineVotingWebApp.VoterViewModels
{
    public class VoteResultViewModel
    {
        public List<CandidatePosition> CandidatePositions { get; set; }
        public List<Candidate> Candidates { get; set; }
        public Dictionary<int, int> PositionVoteCount { get; set; }
        public Dictionary<int, int> CandidateVoteCount { get; set; }
        public int TotalVotes { get; set; }
    }
}
