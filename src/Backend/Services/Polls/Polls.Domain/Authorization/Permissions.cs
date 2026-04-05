namespace Polls.Domain.Authorization;

public static class Permissions
{
    public const string ClaimType = "permissions";

    public static class Cities
    {
        public const string ReadAny = "cities.read.any";
        public const string ReadActive = "cities.read.active";
        public const string CreateAny = "cities.create.any";
        public const string UpdateAny = "cities.update.any";
        public const string UpdateOwn = "cities.update.own";
        public const string DeleteAny = "cities.delete.any";
        public const string ChangeStatusAny = "cities.changestatus.any";
    }

    public static class Polls
    {
        public const string ReadAny = "polls.read.any";
        public const string ReadActive = "polls.read.active";
        public const string CreateOwn = "polls.create.own";
        public const string CreateAny = "polls.create.any";
        public const string UpdateCity = "polls.update.city";
        public const string UpdateAny = "polls.update.any";
        public const string DeleteAny = "polls.delete.any";
        public const string VoteCity = "polls.vote.city";
        public const string VoteAny = "polls.vote.any";
        public const string ChangeStatusAny = "polls.changestatus.any";
    }

    public static class Ideas
    {
        public const string ReadAny = "ideas.read.any";
        public const string ReadActive = "ideas.read.active";
        public const string CreateAny = "ideas.create.any";
        public const string CreateUserVoting = "ideas.create.user-voting";
        public const string CreateManagerVoting = "ideas.create.manager-voting";
        public const string UpdateAny = "ideas.update.any";
        public const string UpdateCity = "ideas.update.city";
        public const string UpdateOwn = "ideas.update.own";
        public const string DeleteAny = "ideas.delete.any";
        public const string DeleteCity = "ideas.delete.city";
        public const string DeleteOwn = "ideas.delete.own";
        public const string ChangeStatusAny = "ideas.changestatus.any";
    }
}
