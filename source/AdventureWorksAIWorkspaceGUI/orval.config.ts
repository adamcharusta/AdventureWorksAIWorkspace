import { defineConfig } from 'orval'

export default defineConfig({
  api: {
    input: {
      target: 'http://localhost:5159/swagger/v1/swagger.json',
    },
    output: {
      mode: 'tags-split',
      target: './src/api/generated/endpoints.ts',
      schemas: './src/api/generated/model',
      client: 'react-query',
      httpClient: 'fetch',
      baseUrl: '',
      clean: true,
      prettier: true,
      mock: {
        type: 'msw',
        delay: 0,
      },
      override: {
        mutator: {
          path: './src/api/customFetch.ts',
          name: 'customFetch',
        },
        query: {
          useQuery: true,
          signal: true,
        },
      },
    },
  },
  apiZod: {
    input: {
      target: 'http://localhost:5159/swagger/v1/swagger.json',
    },
    output: {
      mode: 'tags-split',
      target: './src/api/generated/zod',
      client: 'zod',
      fileExtension: '.zod.ts',
      clean: true,
      prettier: true,
    },
  },
})
