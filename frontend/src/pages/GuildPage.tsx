import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router';
import {
    acceptGuildQuest,
    clearToken,
    getGuildBoard,
    isApiStatusError,
} from '../api/auth';
import type { GuildBoardQuest } from '../api/auth';

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

function GuildPage() {
    const navigate = useNavigate();

    const [guildQuests, setGuildQuests] = useState<GuildBoardQuest[]>([]);
    const [error, setError] = useState('');
    const [message, setMessage] = useState('');
    const [isLoading, setIsLoading] = useState(true);
    const [acceptingQuestId, setAcceptingQuestId] = useState<number | null>(null);

    useEffect(() => {
        async function loadGuildBoard() {
            try {
                setError('');
                const result = await getGuildBoard();
                setGuildQuests(result);
            } catch (err) {
                if (isApiStatusError(err) && err.status === 401) {
                    clearToken();
                    navigate('/login');
                    return;
                }

                setError(err instanceof Error ? err.message : 'Could not load guild board.');
            } finally {
                setIsLoading(false);
            }
        }

        void loadGuildBoard();
    }, [navigate]);

    async function handleAccept(guildQuestId: number) {
        setError('');
        setMessage('');
        setAcceptingQuestId(guildQuestId);

        try {
            const result = await acceptGuildQuest(guildQuestId);
            setMessage(
                `Accepted ${result.name ?? 'guild quest'}. Remaining energy: ${result.remainingEnergy}`,
            );

            const updatedBoard = await getGuildBoard();
            setGuildQuests(updatedBoard);
        } catch (err) {
            if (isApiStatusError(err) && err.status === 401) {
                clearToken();
                navigate('/login');
                return;
            }

            setError(err instanceof Error ? err.message : 'Could not accept guild quest.');
        } finally {
            setAcceptingQuestId(null);
        }
    }

    return (
        <main className="page">
            <section className="page-panel">
                <Link className="back-link" to="/menu">
                    Back to Main Menu
                </Link>

                <h1>Guild Board</h1>

                {isLoading && <p>Loading guild board...</p>}

                {error && <p className="form-error">{error}</p>}

                {message && <p className="form-success">{message}</p>}

                {!isLoading && guildQuests.length === 0 && (
                    <p>No guild quests available today.</p>
                )}

                <section className="quest-list">
                    {guildQuests.map((quest) => {
                        const isUnavailable =
                            quest.isAcceptedToday ||
                            quest.isCompletedToday ||
                            acceptingQuestId !== null;

                        let buttonText = 'Accept';

                        if (quest.isCompletedToday) {
                            buttonText = 'Completed';
                        } else if (quest.isAcceptedToday) {
                            buttonText = 'Accepted';
                        } else if (acceptingQuestId === quest.id) {
                            buttonText = 'Accepting...';
                        }

                        return (
                            <article className="quest-card" key={quest.id}>
                                <div>
                                    <h2><span className="quest-icon">{getEventCategoryIcon(quest.eventCategory)}</span>{quest.name}</h2>
                                    <p>{quest.guildQuestType}</p>
                                    <p>{quest.description}</p>
                                    <p>Reward: {quest.baseXP} XP</p>
                                    <p>Energy Cost: {quest.energyCost}</p>
                                    <p>Required Level: {quest.requiredLevel}</p>
                                </div>

                                <button
                                    type="button"
                                    disabled={isUnavailable}
                                    onClick={() => handleAccept(quest.id)}
                                >
                                    {buttonText}
                                </button>
                            </article>
                        );
                    })}
                </section>
            </section>
        </main>
    );
}

export default GuildPage;