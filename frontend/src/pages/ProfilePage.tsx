import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router';
import { clearToken, getMyPlayer, isApiStatusError } from '../api/auth';
import type { PlayerProfile } from '../api/auth';

function ProfilePage() {
    const navigate = useNavigate();

    const [player, setPlayer] = useState<PlayerProfile | null>(null);
    const [error, setError] = useState('');
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        async function loadPlayer() {
            try {
                const result = await getMyPlayer();
                setPlayer(result);
            } catch (err) {
                setError(err instanceof Error ? err.message : 'Could not load profile.');

                if (isApiStatusError(err) && err.status === 401) {
                    clearToken();
                    navigate('/login');
                }
            } finally {
                setIsLoading(false);
            }
        }

        void loadPlayer();
    }, [navigate]);

    return (
        <main className="page">
            <section className="page-panel">
                <Link className="back-link" to="/menu">
                    Back to Main Menu
                </Link>

                <h1>Profile</h1>

                {isLoading && <p>Loading profile...</p>}

                {error && <p className="form-error">{error}</p>}

                {player && (
                    <section className="profile-summary">
                        <h2 className="profile-name">{player.inGameName}</h2>
                        <div />
                        <div />

                        <p>Level {player.level}</p>
                        <p>Strength: {player.strength}</p>
                        <p>Perception: {player.perception}</p>

                        <p>
                            XP: {player.currentXP} / {player.xpRequiredForNextLevel}
                        </p>
                        <p>Intelligence: {player.intelligence}</p>
                        <p>Luck: {player.luck}</p>

                        <p>Energy: {player.energy} / 100</p>
                        <p>Agility: {player.agility}</p>
                        <div />
                    </section>
                )}
            </section>
        </main>
    );
}

export default ProfilePage;