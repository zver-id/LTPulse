import type { TeamData } from "../types/team-data.ts"

/**
 * Возвращает список линий для графиков.
 * @param teamData Данные команды.
 */
const getAllLines = (teamData: Array<TeamData> | undefined): Array<string> => {
    const lines = new Set<string>();
    teamData?.forEach(item => {
        Object.keys(item).forEach(key => {
            if (key !== 'day') {
                lines.add(key);
            }
        });
    });
    return Array.from(lines);
};

export default getAllLines;