import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router';
import {
    clearToken,
    completeDailyQuest,
    completeGuildQuest,
    getActiveGuildQuests,
    getDailyQuests,
    isApiStatusError,
} from '../api/auth';
import type { ActiveGuildQuest, DailyQuest, QuestOption } from '../api/auth';

function getEventCategoryIcon(eventCategory: string | null) {
    switch (eventCategory) {
        case 'Monster':
            return '⚔';
        case 'Farming':
            return '🌾';
        case 'Mining':
            return '⛏';
        case 'Exploration':
            return '🧭';
        default:
            return '✦';
    }
}

type QuestListItem = {
    source: 'daily' | 'guild';
    id: number;
    name: string | null;
    description: string | null;
    rewardXP: number;
    requiredLevel: number;
    isCompleted: boolean;
    guildQuestType?: string | null;
    energyCost?: number;
    options: QuestOption[];
    eventCategory: string | null;
};

function mapQuests(dailyQuests: DailyQuest[], activeGuildQuests: ActiveGuildQuest[]) {
    const dailyItems = dailyQuests
        .filter((quest: DailyQuest) => !quest.isCompleted)
        .map((quest: DailyQuest): QuestListItem => ({
            source: 'daily',
            id: quest.id,
            name: quest.name,
            description: quest.description,
            rewardXP: quest.baseXP,
            requiredLevel: quest.requiredLevel,
            isCompleted: quest.isCompleted,
            options: quest.options,
            eventCategory: quest.eventCategory
        }));

    const guildItems = activeGuildQuests
        .filter((quest: ActiveGuildQuest) => !quest.isCompleted)
        .map((quest: ActiveGuildQuest): QuestListItem => ({
            source: 'guild',
            id: quest.id,
            name: quest.name,
            description: quest.description,
            rewardXP: quest.baseXP,
            requiredLevel: quest.requiredLevel,
            isCompleted: quest.isCompleted,
            guildQuestType: quest.guildQuestType,
            energyCost: quest.energyCost,
            options: quest.options,
            eventCategory: quest.eventCategory
        }));

    return [...dailyItems, ...guildItems];
}

function QuestsPage() {
    const navigate = useNavigate();

    const [quests, setQuests] = useState<QuestListItem[]>([]);
    const [error, setError] = useState('');
    const [isLoading, setIsLoading] = useState(true);
    const [selectedQuest, setSelectedQuest] = useState<QuestListItem | null>(null);
    const [completingOptionId, setCompletingOptionId] = useState<number | null>(null);
    const [resultMessage, setResultMessage] = useState('');

    useEffect(() => {
        let cancelled = false;

        Promise.all([
            getDailyQuests(),
            getActiveGuildQuests(),
        ])
            .then(([dailyQuests, activeGuildQuests]) => {
                if (cancelled) {
                    return;
                }

                setQuests(mapQuests(dailyQuests, activeGuildQuests));
            })
            .catch((err: unknown) => {
                if (cancelled) {
                    return;
                }

                if (isApiStatusError(err) && err.status === 401) {
                    clearToken();
                    navigate('/login');
                    return;
                }

                setError(err instanceof Error ? err.message : 'Could not load quests.');
            })
            .finally(() => {
                if (!cancelled) {
                    setIsLoading(false);
                }
            });

        return () => {
            cancelled = true;
        };
    }, [navigate]);

    async function reloadQuests() {
        const [dailyQuests, activeGuildQuests] = await Promise.all([
            getDailyQuests(),
            getActiveGuildQuests(),
        ]);
        setQuests(mapQuests(dailyQuests, activeGuildQuests));
    }

    function handleCompleteQuest(optionId: number) {
        if (!selectedQuest) {
            return;
        }

        setError('');
        setResultMessage('');
        setCompletingOptionId(optionId);

        const completionRequest =
            selectedQuest.source === 'daily'
                ? completeDailyQuest(selectedQuest.id, optionId)
                : completeGuildQuest(selectedQuest.id, optionId);

        completionRequest
            .then((result) => {
                let message = `${result.success ? 'Success!' : 'Failed.'} You gained ${result.gainedXP} XP.`;

                if (result.leveledUp) {
                    message += ` Level up! You reached level ${result.newLevel} and gained ${result.statPointsGained} stat points.`;
                }

                setResultMessage(message);

                setSelectedQuest(null);

                return reloadQuests();
            })
            .catch((err: unknown) => {
                if (isApiStatusError(err) && err.status === 401) {
                    clearToken();
                    navigate('/login');
                    return;
                }

                setError(err instanceof Error ? err.message : 'Could not complete quest.');
            })
            .finally(() => {
                setCompletingOptionId(null);
            });
    }

    return (
        <main className="page">
            <section className="page-panel">
                <Link className="back-link" to="/menu">
                    Back to Main Menu
                </Link>

                <h1>Quests</h1>

                {isLoading && <p>Loading quests...</p>}

                {error && <p className="form-error">{error}</p>}

                {resultMessage && <p className="form-success">{resultMessage}</p>}

                {selectedQuest && (
                    <section className="quest-detail">
                        <button type="button" onClick={() => setSelectedQuest(null)}>
                            Back to quest list
                        </button>

                        <h2><span className="quest-icon">{getEventCategoryIcon(selectedQuest.eventCategory)}</span> {selectedQuest.name}</h2>
                        <p>{selectedQuest.description}</p>
                        <p>Reward: {selectedQuest.rewardXP} XP</p>
                        <p>Required Level: {selectedQuest.requiredLevel}</p>

                        <div className="quest-options">
                            {selectedQuest.options.map((option) => (
                                <button
                                    key={option.id}
                                    type="button"
                                    disabled={completingOptionId !== null}
                                    onClick={() => handleCompleteQuest(option.id)}
                                >
                                    {completingOptionId === option.id
                                        ? 'Resolving...'
                                        : option.text}
                                </button>
                            ))}
                        </div>
                    </section>
                )}

                {!selectedQuest && !isLoading && quests.length === 0 && (
                    <p>No active quests right now. Visit the guild board or come back tomorrow.</p>
                )}

                {!selectedQuest && (
                    <section className="quest-list">
                        {quests.map((quest) => (
                            <article className="quest-card" key={`${quest.source}-${quest.id}`}>
                                <div>
                                    <h2><span className="quest-icon">{getEventCategoryIcon(quest.eventCategory)}</span> {quest.name}</h2>
                                    <p>{quest.description}</p>
                                    <p>Reward: {quest.rewardXP} XP</p>
                                    <p>Required Level: {quest.requiredLevel}</p>
                                </div>

                                <button type="button" onClick={() => setSelectedQuest(quest)}>
                                    Let's do it!
                                </button>
                            </article>
                        ))}
                    </section>
                )}
            </section>
        </main>
    );
}

export default QuestsPage;