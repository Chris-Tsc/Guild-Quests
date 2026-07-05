using DAL.Models;

namespace DAL.Repositories.Interfaces
{
    public interface IGuildQuestRepository
    {
        Task<PlayerRolledGuildQuest?> GetQuestFromBoardAsync(int playerId, int guildQuestId, DateTime dayUtc);
        Task<bool> HasAcceptedQuestTodayAsync(int playerId, int guildQuestId, DateTime dayUtc);
        Task<List<PlayerGuildQuest>> GetAcceptedTodayAsync(int playerId, DateTime dayUtc);
        Task<List<PlayerGuildQuest>> GetActiveAcceptedQuestsAsync(int playerId, DateTime dayUtc);
        Task<PlayerGuildQuest?> GetAcceptedQuestCheckForCompletionAsync(int playerId, int guildQuestId, DateTime dayUtc);
        Task<GuildQuestOption?> GetOptionForQuestAsync(int guildQuestId, int optionId);

        Task<List<PlayerRolledGuildQuest>> GetBoardTodayAsync(int playerId, DateTime dayUtc);
        Task<List<GuildQuest>> GetEligibleGuildQuestsAsync(int playerLevel);

        void AddAcceptedQuest(PlayerGuildQuest acceptedQuest);
        void RemoveLeftoverRolledBoard(List<PlayerRolledGuildQuest> board);
        void AddRolledBoard(List<PlayerRolledGuildQuest> board);
    }
}
