import type {Team} from "./team.ts";

export type NavigationProps = {
    team: Team | null;
    onChange: (team: string) => void;
}