import type {TeamData} from "../types/team-data.ts";
import getAllLines from "./getAllLines.ts";

const findMaxValue = (data:TeamData[]) => {
    let maxValue = 0;
    const categories = getAllLines(data)
    data.forEach(element => {
        categories.forEach(category => {
            if (typeof element[category] === "number") {
                maxValue = Math.max(maxValue, element[category]);
            }
        })
    })
    return maxValue;
};

export default findMaxValue;