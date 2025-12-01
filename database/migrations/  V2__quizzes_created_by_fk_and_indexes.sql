BEGIN;

-- Sørg for at quizzes har created_at (hvis V1 er ændret hos jer)
ALTER TABLE public.quizzes
  ADD COLUMN IF NOT EXISTS created_at timestamptz NOT NULL DEFAULT now();

-- Tilføj created_by, hvis den mangler
ALTER TABLE public.quizzes
  ADD COLUMN IF NOT EXISTS created_by uuid;

-- FK til users(id) med ON DELETE CASCADE (tilføjes kun én gang)
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM   information_schema.table_constraints tc
    WHERE  tc.table_schema = 'public'
      AND  tc.table_name   = 'quizzes'
      AND  tc.constraint_type = 'FOREIGN KEY'
      AND  tc.constraint_name = 'fk_quizzes_created_by_users'
  ) THEN
    ALTER TABLE public.quizzes
      ADD CONSTRAINT fk_quizzes_created_by_users
      FOREIGN KEY (created_by)
      REFERENCES public.users(id)
      ON DELETE CASCADE;
  END IF;
END$$;

-- Gør created_by NOT NULL kun hvis alle rækker allerede har værdi (skånsomt)
DO $$
BEGIN
  IF EXISTS (
    SELECT 1
    FROM information_schema.columns
    WHERE table_schema='public' AND table_name='quizzes' AND column_name='created_by'
  ) THEN
    IF NOT EXISTS (SELECT 1 FROM public.quizzes WHERE created_by IS NULL) THEN
      ALTER TABLE public.quizzes
        ALTER COLUMN created_by SET NOT NULL;
    END IF;
  END IF;
END$$;

-- Indekser til hurtige opslag
CREATE INDEX IF NOT EXISTS ix_quizzes_created_by
  ON public.quizzes(created_by);

-- Find "mine quizzer – nyeste først"
CREATE INDEX IF NOT EXISTS ix_quizzes_owner_created_at_desc
  ON public.quizzes(created_by, created_at DESC);

-- Hjælpeindekser til options
CREATE INDEX IF NOT EXISTS ix_question_options_question_id
  ON public.question_options(question_id);

CREATE INDEX IF NOT EXISTS ix_question_options_question_id_is_correct
  ON public.question_options(question_id, is_correct);

COMMIT;
