import js from '@eslint/js'
import { defineConfig, globalIgnores } from 'eslint/config'
import prettierRecommended from 'eslint-plugin-prettier/recommended'
import jsxA11y from 'eslint-plugin-jsx-a11y'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import simpleImportSort from 'eslint-plugin-simple-import-sort'
import unusedImports from 'eslint-plugin-unused-imports'
import globals from 'globals'
import tseslint from 'typescript-eslint'

const cypressGlobals = {
  cy: 'readonly',
  Cypress: 'readonly',
  before: 'readonly',
  beforeEach: 'readonly',
  after: 'readonly',
  afterEach: 'readonly',
  describe: 'readonly',
  context: 'readonly',
  it: 'readonly',
  specify: 'readonly',
  expect: 'readonly',
  assert: 'readonly',
  chai: 'readonly',
}

export default defineConfig([
  globalIgnores([
    'dist',
    'src/api/generated',
    'cypress/screenshots',
    'cypress/videos',
    'cypress/downloads',
    'coverage',
  ]),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      js.configs.recommended,
      tseslint.configs.recommended,
      reactHooks.configs.flat.recommended,
      reactRefresh.configs.vite,
      jsxA11y.flatConfigs.recommended,
      prettierRecommended,
    ],
    plugins: {
      'unused-imports': unusedImports,
      'simple-import-sort': simpleImportSort,
    },
    languageOptions: {
      globals: globals.browser,
    },
    rules: {
      '@typescript-eslint/no-unused-vars': 'off',
      'unused-imports/no-unused-imports': 'error',
      'unused-imports/no-unused-vars': [
        'warn',
        {
          vars: 'all',
          varsIgnorePattern: '^_',
          args: 'after-used',
          argsIgnorePattern: '^_',
        },
      ],
      'simple-import-sort/imports': 'error',
      'simple-import-sort/exports': 'error',
    },
  },
  {
    files: ['cypress/**/*.{ts,tsx}'],
    languageOptions: {
      globals: cypressGlobals,
    },
    rules: {
      'react-refresh/only-export-components': 'off',
    },
  },
])
