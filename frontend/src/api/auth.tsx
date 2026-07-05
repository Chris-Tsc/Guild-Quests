const API_BASE_URL = 'https://localhost:7164/api';

type ApiErrorResponse = {
    error?: string;
};

export type ApiStatusError = Error & {
    status?: number;
};

export function getToken() {
    return localStorage.getItem('guildQuestsToken');
}

export function setToken(token: string) {
    localStorage.setItem('guildQuestsToken', token);
}

export function clearToken() {
    localStorage.removeItem('guildQuestsToken');
}

export async function apiRequest<TResponse>(
    path: string,
    options: RequestInit = {},
): Promise<TResponse> {
    const token = getToken();

    const response = await fetch(`${API_BASE_URL}${path}`, {
        ...options,
        headers: {
            'Content-Type': 'application/json',
            ...(token ? { Authorization: `Bearer ${token}` } : {}),
            ...options.headers,
        },
    });

    if (!response.ok) {
        const errorBody = (await response.json().catch(() => ({}))) as ApiErrorResponse;
        const error = new Error(
            errorBody.error ?? `Request failed with status ${response.status}`,
        ) as ApiStatusError;

        error.status = response.status;

        throw error;
    }

    return await response.json() as Promise<TResponse>;
}

export type PlayerProfile = {
    id: number;
    inGameName: string;
    level: number;
    currentXP: number;
    unspentStatPoints: number;
    xpRequiredForNextLevel: number;
    energy: number;
    strength: number;
    intelligence: number;
    agility: number;
    perception: number;
    luck: number;
};

export async function getMyPlayer() {
    return apiRequest<PlayerProfile>('/Player/authotest');
}

export function isApiStatusError(error: unknown): error is ApiStatusError {
    return error instanceof Error && 'status' in error;
}

export async function spendStatPoints(stat: string, points: number) {
    return apiRequest<PlayerProfile>('/Player/spend-stat-points', {
        method: 'POST',
        body: JSON.stringify({ stat, points }),
    });
}

export type GuildBoardQuest = {
    id: number;
    name: string | null;
    description: string | null;
    guildQuestType: string | null;
    requiredLevel: number;
    energyCost: number;
    eventsId: number;
    baseXP: number;
    isAcceptedToday: boolean;
    isCompletedToday: boolean;
    eventCategory: string | null;
};

export type AcceptGuildQuestResult = {
    guildQuestId: number;
    name: string | null;
    guildQuestType: string | null;
    energySpent: number;
    remainingEnergy: number;
};

export async function getGuildBoard() {
    return apiRequest<GuildBoardQuest[]>('/GuildQuest/board');
}

export async function acceptGuildQuest(guildQuestId: number) {
    return apiRequest<AcceptGuildQuestResult>('/GuildQuest/accept', {
        method: 'POST',
        body: JSON.stringify({ guildQuestId }),
    });
}

export type QuestOption = {
    id: number;
    text: string | null;
};

export type DailyQuest = {
    id: number;
    name: string | null;
    description: string | null;
    baseXP: number;
    requiredLevel: number;
    eventsId: number;
    isCompleted: boolean;
    options: QuestOption[];
    eventCategory: string | null;
};

export type ActiveGuildQuest = {
    id: number;
    name: string | null;
    description: string | null;
    guildQuestType: string | null;
    requiredLevel: number;
    energyCost: number;
    eventsId: number;
    baseXP: number;
    isCompleted: boolean;
    options: QuestOption[];
    eventCategory: string | null;
};

export async function getDailyQuests() {
    return apiRequest<DailyQuest[]>('/DailyQuest/today');
}

export async function getActiveGuildQuests() {
    return apiRequest<ActiveGuildQuest[]>('/GuildQuest/active');
}

export type CompleteQuestResult = {
    success: boolean;
    gainedXP: number;
    newCurrentXP: number;
    newLevel: number;
    xpRequiredForNextLevel: number;
    leveledUp: boolean;
    statPointsGained: number;
};

export async function completeDailyQuest(dailyQuestId: number, dailyQuestOptionId: number) {
    return apiRequest<CompleteQuestResult>('/DailyQuest/complete', {
        method: 'POST',
        body: JSON.stringify({ dailyQuestId, dailyQuestOptionId }),
    });
}

export async function completeGuildQuest(guildQuestId: number, guildQuestOptionId: number) {
    return apiRequest<CompleteQuestResult>('/GuildQuest/complete', {
        method: 'POST',
        body: JSON.stringify({ guildQuestId, guildQuestOptionId }),
    });
}