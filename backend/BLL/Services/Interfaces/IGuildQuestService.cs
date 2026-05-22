using BLL.Contracts.GuildQuests;

namespace BLL.Services.Interfaces
{
    public interface IGuildQuestService
    {
        Task<List<GuildQuestBoardTestDto>> GetTodayGuildBoardAsync(string appUserId);

        Task<AcceptGuildQuestResultDto> AcceptGuildQuestAsync(string appUserId, int guildQuestId);

        Task<List<ActiveGuildQuestDto>> GetActiveGuildQuestsAsync(string appUserId);

        Task<CompleteGuildQuestResultDto> CompleteGuildQuestAsync(
            string appUserId,
            int guildQuestId,
            int guildQuestOptionId);
    }
}
