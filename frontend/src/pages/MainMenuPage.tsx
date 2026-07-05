import { useMemo, useEffect } from 'react';
import { Link, useNavigate } from 'react-router';
import { clearToken, getToken } from '../api/auth';

function MainMenuPage() {
    const navigate = useNavigate();

    useEffect(() => {
        if (!getToken()) {
            navigate('/login');
        }
    }, [navigate]);

    const isLoggedIn = useMemo(() => {
        return Boolean(getToken());
    }, []);

    const boxes = [
        { title: 'Guild', path: '/guild' },
        { title: 'Quests', path: '/quests' },
        { title: 'Stats', path: '/stats' },
        { title: 'Profile', path: '/profile' },
    ];

    function handleLogout() {
        clearToken();
        navigate('/login');
    }

    return (
        <div className="page">
            <div className="dashboard">
                {boxes.map((box) => (
                    <Link key={box.title} className="box" to={box.path}>
                        {box.title}
                    </Link>
                ))}

                <button
                    className="box menu-button"
                    type="button"
                    disabled={!isLoggedIn}
                    onClick={handleLogout}
                >
                    Logout
                </button>
            </div>
        </div>
    );
}

export default MainMenuPage;