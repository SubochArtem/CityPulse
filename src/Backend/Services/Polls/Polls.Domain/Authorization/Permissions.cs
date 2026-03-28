namespace Polls.Domain.Authorization;

public static class Permissions
{
    public const string ClaimType = "permissions";

    public static class Cities
    {
        public const string ReadAny = "cities.read.any";
        public const string CreateAny = "cities.create.any";
        public const string UpdateAny = "cities.update.any";
        public const string DeleteAny = "cities.delete.any";
        public const string UpdateOwn = "cities.update.own";
    }

    public static class Polls
    {
        public const string CreateOwn = "poll.create.own";
        public const string CreateAny = "poll.create.any";
        public const string UpdateCity = "poll.update.city";
        public const string UpdateAny = "poll.update.any";
        public const string VoteCity = "poll.vote.city";
        public const string VoteAny = "poll.vote.any";
    }

    public static class Ideas
    {
        public const string CreateAny = "ideas.create.any";
        public const string CreateUserVoting = "ideas.create.user-voting";
        public const string CreateFinalVoting = "ideas.create.final-voting";
        public const string ReadAny = "ideas.read.any";
        public const string UpdateAny = "ideas.update.any";
        public const string UpdateCity = "ideas.update.city";
        public const string UpdateOwn = "ideas.update.own";
        public const string DeleteAny = "ideas.delete.any";
        public const string DeleteCity = "ideas.delete.city";
        public const string DeleteOwn = "ideas.delete.own";
    }
}
