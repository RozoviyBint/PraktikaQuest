async function getTree() {
    const url = `/api/story/tree`;

    try {
        const response = await fetch(url, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error('Не удалось получить данные: ' + response.statusText);
        }

        const tree = await response.json();
        console.log(tree.scenes);
        return tree.scenes;
    } catch (error) {
        console.error('Ошибка при запросе:', error);
    }
}

async function getAvailableActions(blockId) {
    const url = `/api/story/available-actions?blockId=${blockId}`;

    try {
        const response = await fetch(url, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error('Не удалось получить доступные действия: ' + response.statusText);
        }
        console.log("Выполнен запрос на получение действий");
        return await response.json();
    } catch (error) {
        console.error('Ошибка при запросе доступных действий:', error);
    }
}

async function completeEvent(blockId, actionId, visitedBlocks) {
    const url = `/api/story/event-complete`;

    const requestData = {
        blockId: blockId,
        eventId: 0, // Всегда 0
        actionId: actionId,
        newBlocks: visitedBlocks
    };

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestData)
        });

        if (!response.ok) {
            throw new Error('Ошибка при выполнении действия: ' + response.statusText);
        }

        return await response.json();
    } catch (error) {
        console.error('Ошибка при отправке запроса на выполнение действия:', error);
    }
}

// Функция обновления сцен, чтобы избежать дубликатов
function updateScenes(existingScenes, newScenes) {
    const sceneMap = new Map(existingScenes.map(scene => [scene.id, scene]));

    newScenes.forEach(scene => {
        if (!sceneMap.has(scene.id)) {
            sceneMap.set(scene.id, scene);
        }
    });

    return Array.from(sceneMap.values());
}

async function initializeQuest() {
    let scenes = await getTree();
    const visitedBlocks = []; // Массив посещенных блоков

    if (!Array.isArray(scenes)) {
        console.error('Полученные данные не являются массивом сцен');
        return;
    }

    scenes.forEach(scene => {
        scene.descriptionShown = false;
    });

    let currentSceneIndex = 0;
    let currentDescriptionIndex = 0;

    async function renderScene(sceneIndex) {
        const scene = scenes[sceneIndex];
        const sceneContainer = document.getElementById('scene-container');
        sceneContainer.innerHTML = '';

        if (!visitedBlocks.includes(scene.id)) {
            visitedBlocks.push(scene.id);
            console.log('Посещенные блоки:', visitedBlocks);
        }

        // Фон сцены
        const background = document.createElement('div');
        background.className = 'background-image';
        background.style.backgroundImage = `url('${scene.backgroundImage}')`;
        sceneContainer.appendChild(background);

        if (!scene.descriptionShown) {
            // Описание сцены
            const sceneDescription = document.createElement('div');
            sceneDescription.className = 'scene-description';
            sceneDescription.textContent = scene.description[currentDescriptionIndex];
            sceneContainer.appendChild(sceneDescription);

            // Кнопка "Продолжить"
            const continueButton = document.createElement('button');
            continueButton.className = 'continue-button';
            continueButton.textContent = 'Продолжить';
            continueButton.onclick = () => {
                currentDescriptionIndex++;
                if (currentDescriptionIndex < scene.description.length) {
                    renderScene(sceneIndex);
                } else {
                    scene.descriptionShown = true;
                    renderScene(sceneIndex);
                }
            };
            sceneDescription.appendChild(continueButton);
        } else {
            const optionsContainer = document.createElement('div');
            optionsContainer.className = 'options-container';

            // Запрашиваем доступные действия, если есть событие
            if (scene.event) {
                const actions = await getAvailableActions(scene.id);
                if (actions && actions.length > 0) {
                    actions.forEach(action => {
                        const actionButton = document.createElement('button');
                        actionButton.className = 'route-button'; // Используем стиль маршрута
                        actionButton.textContent = action.description;
                        actionButton.onclick = async () => {
                            const result = await completeEvent(scene.id, action.id, visitedBlocks);
                            console.log('Действие выполнено:', action);

                            // Обновляем дерево сцен, исключая дубликаты
                            if (result && result.scenes) {
                                scenes = updateScenes(scenes, result.scenes);
                            }

                            // Определяем следующую сцену
                            const nextSceneId = action.resultBlockId;
                            const nextSceneIndex = scenes.findIndex(scene => scene.id === nextSceneId);

                            if (nextSceneIndex !== -1) {
                                currentSceneIndex = nextSceneIndex;
                                currentDescriptionIndex = 0;
                                renderScene(currentSceneIndex);
                            }
                        };
                        optionsContainer.appendChild(actionButton);
                    });
                }
            }

            // Добавляем кнопки маршрутов
            scene.routes.forEach(route => {
                const button = document.createElement('button');
                button.className = 'route-button';
                button.textContent = route.routeDescription;
                button.onclick = () => handleRouteClick(route.targetBlockId);
                optionsContainer.appendChild(button);
            });

            // Добавляем анимацию появления кнопок
            optionsContainer.classList.add('animate-buttons');
            sceneContainer.appendChild(optionsContainer);
        }
    }

    function handleRouteClick(targetBlockId) {
        const nextSceneIndex = scenes.findIndex(scene => scene.id === targetBlockId);
        if (nextSceneIndex !== -1) {
            currentSceneIndex = nextSceneIndex;
            currentDescriptionIndex = 0;
            renderScene(currentSceneIndex);
        }
    }

    renderScene(currentSceneIndex);
}

// Инициализация квеста
initializeQuest();
