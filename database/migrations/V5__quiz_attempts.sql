BEGIN;

-- =====================================================
-- Quiz Attempts - Gemmer hvert forsøg en bruger laver
-- =====================================================
CREATE TABLE IF NOT EXISTS public.quiz_attempts (
    id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    quiz_id         uuid NOT NULL REFERENCES public.quizzes(id) ON DELETE CASCADE,
    user_id         uuid NOT NULL REFERENCES public.users(id) ON DELETE CASCADE,
    score           int NOT NULL DEFAULT 0,
    total_points    int NOT NULL DEFAULT 0,
    correct_count   int NOT NULL DEFAULT 0,
    total_count     int NOT NULL DEFAULT 0,
    duration_seconds int NOT NULL DEFAULT 0,
    started_at      timestamptz NOT NULL DEFAULT now(),
    completed_at    timestamptz NOT NULL DEFAULT now(),
    is_best_attempt boolean NOT NULL DEFAULT false,
    created_at      timestamptz NOT NULL DEFAULT now()
);

-- =====================================================
-- Attempt Answers - Gemmer hvert svar i et forsøg
-- =====================================================
CREATE TABLE IF NOT EXISTS public.attempt_answers (
    id                  uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    attempt_id          uuid NOT NULL REFERENCES public.quiz_attempts(id) ON DELETE CASCADE,
    question_id         uuid NOT NULL REFERENCES public.quiz_questions(id) ON DELETE CASCADE,
    selected_option_id  uuid REFERENCES public.question_options(id) ON DELETE SET NULL,
    is_correct          boolean NOT NULL DEFAULT false,
    points_earned       int NOT NULL DEFAULT 0,
    created_at          timestamptz NOT NULL DEFAULT now()
);

-- =====================================================
-- Tilføj total_score til users tabellen
-- =====================================================
ALTER TABLE public.users 
    ADD COLUMN IF NOT EXISTS total_score int NOT NULL DEFAULT 0;

-- =====================================================
-- Indexes for performance
-- =====================================================

-- Find alle forsøg for en bruger
CREATE INDEX IF NOT EXISTS ix_quiz_attempts_user_id 
    ON public.quiz_attempts(user_id);

-- Find alle forsøg for en quiz
CREATE INDEX IF NOT EXISTS ix_quiz_attempts_quiz_id 
    ON public.quiz_attempts(quiz_id);

-- Find brugers forsøg på en specifik quiz (sorteret efter score)
CREATE INDEX IF NOT EXISTS ix_quiz_attempts_user_quiz_score 
    ON public.quiz_attempts(user_id, quiz_id, score DESC);

-- Find bedste forsøg hurtigt
CREATE INDEX IF NOT EXISTS ix_quiz_attempts_best 
    ON public.quiz_attempts(user_id, quiz_id) 
    WHERE is_best_attempt = true;

-- Find svar for et forsøg
CREATE INDEX IF NOT EXISTS ix_attempt_answers_attempt_id 
    ON public.attempt_answers(attempt_id);

COMMIT;
