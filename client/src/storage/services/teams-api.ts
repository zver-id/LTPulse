import {createApi, fetchBaseQuery} from '@reduxjs/toolkit/query/react'
import apiUrl from "./api-url.ts";
import type {Team} from "../../types/team.ts";

export const teamsApi = createApi({
  reducerPath: "teams",
  baseQuery: fetchBaseQuery({baseUrl: `${apiUrl}/api`}),
  endpoints: (builder) => ({
    getAllTeams: builder.query<Team[], void>({
      query: () => `teams`,
    })
  })
});

export const {useGetAllTeamsQuery} = teamsApi;
