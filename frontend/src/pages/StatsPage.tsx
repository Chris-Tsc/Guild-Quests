import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router';
import {
    clearToken,
    getMyPlayer,
    isApiStatusError,
    spendStatPoints,
} from '../api/auth';
import type { PlayerProfile } from '../api/auth';

type StatName = 'Strength' | 'Intelligence' | 'Agility' | 'Perception' | 'Luck';

function StatsPage() {
    const navigate = useNavigate();

    const [player, setPlayer] = useState<PlayerProfile | null>(null);
    const [error, setError] = useState('');
    const [isLoading, setIsLoading] = useState(true);
    const [spendingStat, setSpendingStat] = useState<StatName | null>(null);

    useEffect(() => {
        async function loadPlayer() {
            try {
                const result = await getMyPlayer();
                setPlayer(result);
            } catch (err) {
                if (isApiStatusError(err) && err.status === 401) {
                    clearToken();
                    navigate('/login');
                    return;
                }

                setError(err instanceof Error ? err.message : 'Could not load stats.');
            } finally {
                setIsLoading(false);
            }
        }

        void loadPlayer();
    }, [navigate]);

    async function handleSpend(stat: StatName) {
        setError('');
        setSpendingStat(stat);

        try {
            const updatedPlayer = await spendStatPoints(stat, 1);
            setPlayer(updatedPlayer);
        } catch (err) {
            if (isApiStatusError(err) && err.status === 401) {
                clearToken();
                navigate('/login');
                return;
            }

            setError(err instanceof Error ? err.message : 'Could not spend stat point.');
        } finally {
            setSpendingStat(null);
        }
    }

    const stats = player
        ? [
            { name: 'Strength' as const, value: player.strength },
            { name: 'Intelligence' as const, value: player.intelligence },
            { name: 'Agility' as const, value: player.agility },
            { name: 'Perception' as const, value: player.perception },
            { name: 'Luck' as const, value: player.luck },
        ]
        : [];

    const canSpend = Boolean(player && player.unspentStatPoints > 0);

    return (
        <main className="page">
            <section className="page-panel">
                <Link className="back-link" to="/menu">
                    Back to Main Menu
                </Link>

                <h1>Stats</h1>

                {isLoading && <p>Loading stats...</p>}

                {error && <p className="form-error">{error}</p>}

                {player && (
                    <>
                        <div className="stat-points">
                            Available Stat Points: {player.unspentStatPoints}
                        </div>

                        <section className="stats-row">
                            {stats.map((stat) => (
                                <article className="stat-card" key={stat.name}>
                                    <button
                                        type="button"
                                        disabled={!canSpend || spendingStat !== null}
                                        onClick={() => handleSpend(stat.name)}
                                    >
                                        {spendingStat === stat.name ? '...' : '+'}
                                    </button>

                                    <h2>{stat.name}</h2>
                                    <p>{stat.value}</p>
                                </article>
                            ))}
                        </section>
                    </>
                )}
            </section>
        </main>
    );
}

export default StatsPage;