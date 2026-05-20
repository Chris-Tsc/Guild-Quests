using BLL.Contracts.GuildQuests;

namespace BLL.Services.Interfaces
{
    public interface IGuildQuestService
    {
        Task<List<GuildQuestBoardTestDto>> GetTodayGuildBoardAsync(string appUserId);

        Task<AcceptGuildQuestResultDto> AcceptGuildQuestAsync(string appUserId, int guildQuestId);
    }
}
